using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Services;

/// <summary>
/// Enterprise ticket routing engine implementing the 4-tier priority cascade:
/// 
/// 1. EXPLICIT COMPANY ROUTING — Evaluates rules for ticket.CompanyId ordered by RulePriority.
///    Each rule can filter on ModuleId, RegionCode, TierCode, Priority, TicketType, Keywords.
///    First fully-matching rule wins.
/// 
/// 2. CATEGORY ROUTING — If no explicit rule matched, find any company routing rule
///    that matches only on ModuleId (broadest category match).
/// 
/// 3. REGION ROUTING — Load company's GeoRegion, find a group mapped to the company
///    where Group.RegionCode matches company.GeoRegion.
/// 
/// 4. FALLBACK — Company's default group (company_groups.is_default), then system fallback.
/// 
/// After resolving a group, auto-assigns an agent via the group's configured assignment strategy.
/// </summary>
public sealed class TicketRoutingEngine : ITicketRoutingEngine
{
    private readonly IRoutingRuleRepository _routingRules;
    private readonly ITicketRoutingContextRepository _routingContext;
    private readonly ITicketRepository _ticketRepo;
    private readonly AgentAssignmentStrategyFactory _strategyFactory;

    public TicketRoutingEngine(
        IRoutingRuleRepository routingRules,
        ITicketRoutingContextRepository routingContext,
        ITicketRepository ticketRepo,
        AgentAssignmentStrategyFactory strategyFactory)
    {
        _routingRules = routingRules;
        _routingContext = routingContext;
        _ticketRepo = ticketRepo;
        _strategyFactory = strategyFactory;
    }

    public async Task<RoutingResult> RouteAsync(Ticket ticket, CancellationToken ct = default)
    {
        // ── Tier 1: Explicit Company Routing Rules ───────────────────────────
        var rules = await _routingRules.GetByCompanyOrderedAsync(ticket.CompanyId, ct);

        // Get company region for rule matching
        string? companyRegion = null;
        try { companyRegion = await _ticketRepo.GetCompanyRegionAsync(ticket.CompanyId, ct); }
        catch { /* company may not have region */ }

        foreach (var rule in rules.Where(r => !r.IsDefault))
        {
            if (RuleMatches(rule, ticket, companyRegion))
            {
                var agentId = await AutoAssignAgent(rule.GroupId, ct);
                return new RoutingResult(
                    rule.GroupId,
                    agentId,
                    "CompanyExplicit",
                    $"Rule priority {rule.RulePriority}: Module={rule.ModuleId}, Region={rule.RegionCode}");
            }
        }

        // ── Tier 2: Category (Module) Routing ────────────────────────────────
        if (ticket.ModuleId != Guid.Empty)
        {
            var categoryRule = rules.FirstOrDefault(r =>
                !r.IsDefault
                && r.ModuleId == ticket.ModuleId
                && r.RegionCode == null
                && r.TierCode == null
                && r.Priority == null
                && r.TicketType == null);

            if (categoryRule is not null)
            {
                var agentId = await AutoAssignAgent(categoryRule.GroupId, ct);
                return new RoutingResult(
                    categoryRule.GroupId,
                    agentId,
                    "CategoryMatch",
                    $"Category/Module match: {ticket.ModuleId}");
            }
        }

        // ── Tier 3: Region Routing ───────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(companyRegion))
        {
            var companyGroups = await _routingContext.GetCompanyGroupsAsync(ticket.CompanyId, ct);
            foreach (var groupId in companyGroups)
            {
                var groupInfo = await _routingContext.GetGroupRoutingInfoAsync(groupId, ct);
                if (groupInfo.HasValue && string.Equals(groupInfo.Value.RegionCode, companyRegion, StringComparison.OrdinalIgnoreCase))
                {
                    var agentId = await AutoAssignAgent(groupId, ct);
                    return new RoutingResult(
                        groupId,
                        agentId,
                        "RegionMatch",
                        $"Region match: {companyRegion}");
                }
            }
        }

        // ── Tier 4: Fallback ─────────────────────────────────────────────────

        // 4a: Company's default routing rule
        var defaultRule = rules.FirstOrDefault(r => r.IsDefault);
        if (defaultRule is not null)
        {
            var agentId = await AutoAssignAgent(defaultRule.GroupId, ct);
            return new RoutingResult(
                defaultRule.GroupId,
                agentId,
                "Fallback",
                $"Company default routing rule → Group {defaultRule.GroupId}");
        }

        // 4b: Company's default group mapping
        var defaultGroupId = await _routingContext.GetCompanyDefaultGroupAsync(ticket.CompanyId, ct);
        if (defaultGroupId.HasValue)
        {
            var agentId = await AutoAssignAgent(defaultGroupId.Value, ct);
            return new RoutingResult(
                defaultGroupId.Value,
                agentId,
                "Fallback",
                $"Company default group mapping → Group {defaultGroupId.Value}");
        }

        // 4c: Any group mapped to this company (first by priority)
        var anyGroupId = (await _routingContext.GetCompanyGroupsAsync(ticket.CompanyId, ct)).FirstOrDefault();
        if (anyGroupId != Guid.Empty)
        {
            var agentId = await AutoAssignAgent(anyGroupId, ct);
            return new RoutingResult(
                anyGroupId,
                agentId,
                "Fallback",
                $"First available company group → Group {anyGroupId}");
        }

        // 4d: No route found — ticket lands in global unassigned
        return new RoutingResult(null, null, "None", "No routing rule or group mapping found for this company.");
    }

    public async Task<Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto> SimulateAsync(Ticket ticket, CancellationToken ct = default)
    {
        var traces = new List<Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto>();
        
        // ── Tier 1: Explicit Company Routing Rules ───────────────────────────
        var rules = await _routingRules.GetByCompanyOrderedAsync(ticket.CompanyId, ct);

        string? companyRegion = null;
        try { companyRegion = await _ticketRepo.GetCompanyRegionAsync(ticket.CompanyId, ct); }
        catch { /* company may not have region */ }

        foreach (var rule in rules.Where(r => !r.IsDefault))
        {
            if (RuleMatches(rule, ticket, companyRegion))
            {
                traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                    "CompanyExplicit", true, $"Matched explicit rule (Priority {rule.RulePriority})", rule.Id));
                var agentId = await AutoAssignAgent(rule.GroupId, ct);
                return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                    rule.GroupId, agentId, "CompanyExplicit", $"Matched explicit rule (Priority {rule.RulePriority})", traces.AsReadOnly());
            }
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "CompanyExplicit", false, $"Failed to match explicit rule (Priority {rule.RulePriority})", rule.Id));
        }

        // ── Tier 2: Category (Module) Routing ────────────────────────────────
        if (ticket.ModuleId != Guid.Empty)
        {
            var categoryRule = rules.FirstOrDefault(r =>
                !r.IsDefault
                && r.ModuleId == ticket.ModuleId
                && r.RegionCode == null
                && r.TierCode == null
                && r.Priority == null
                && r.TicketType == null);

            if (categoryRule is not null)
            {
                traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                    "CategoryMatch", true, $"Matched broader category/module rule for module {ticket.ModuleId}", categoryRule.Id));
                var agentId = await AutoAssignAgent(categoryRule.GroupId, ct);
                return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                    categoryRule.GroupId, agentId, "CategoryMatch", $"Matched category/module rule for module {ticket.ModuleId}", traces.AsReadOnly());
            }
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "CategoryMatch", false, $"No broad category/module rule found for module {ticket.ModuleId}"));
        }

        // ── Tier 3: Region Routing ───────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(companyRegion))
        {
            var companyGroups = await _routingContext.GetCompanyGroupsAsync(ticket.CompanyId, ct);
            foreach (var groupId in companyGroups)
            {
                var groupInfo = await _routingContext.GetGroupRoutingInfoAsync(groupId, ct);
                if (groupInfo.HasValue && string.Equals(groupInfo.Value.RegionCode, companyRegion, StringComparison.OrdinalIgnoreCase))
                {
                    traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                        "RegionMatch", true, $"Matched group {groupId} region ({groupInfo.Value.RegionCode}) with company region ({companyRegion})"));
                    var agentId = await AutoAssignAgent(groupId, ct);
                    return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                        groupId, agentId, "RegionMatch", $"Matched group {groupId} region with company region", traces.AsReadOnly());
                }
            }
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "RegionMatch", false, $"No matching group region found for company region ({companyRegion})"));
        }

        // ── Tier 4: Fallback ─────────────────────────────────────────────────
        var defaultRule = rules.FirstOrDefault(r => r.IsDefault);
        if (defaultRule is not null)
        {
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "Fallback", true, $"Used company default routing rule targeting group {defaultRule.GroupId}", defaultRule.Id));
            var agentId = await AutoAssignAgent(defaultRule.GroupId, ct);
            return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                defaultRule.GroupId, agentId, "Fallback", $"Used company default routing rule targeting group {defaultRule.GroupId}", traces.AsReadOnly());
        }

        var defaultGroupId = await _routingContext.GetCompanyDefaultGroupAsync(ticket.CompanyId, ct);
        if (defaultGroupId.HasValue)
        {
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "Fallback", true, $"Used company default group mapping {defaultGroupId.Value}"));
            var agentId = await AutoAssignAgent(defaultGroupId.Value, ct);
            return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                defaultGroupId.Value, agentId, "Fallback", $"Used company default group mapping", traces.AsReadOnly());
        }

        var anyGroupId = (await _routingContext.GetCompanyGroupsAsync(ticket.CompanyId, ct)).FirstOrDefault();
        if (anyGroupId != Guid.Empty)
        {
            traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
                "Fallback", true, $"Used first available company group {anyGroupId}"));
            var agentId = await AutoAssignAgent(anyGroupId, ct);
            return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
                anyGroupId, agentId, "Fallback", $"Used first available company group", traces.AsReadOnly());
        }

        traces.Add(new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingTraceDto(
            "Fallback", false, $"No default rule, default group, or any mapped group found"));

        return new Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto(
            null, null, "None", "No routing rule or group mapping found", traces.AsReadOnly());
    }

    private bool RuleMatches(CompanyRoutingRule rule, Ticket ticket, string? companyRegion)
    {
        // Every non-null filter dimension must match; null dimensions are "any"
        if (rule.ModuleId.HasValue && rule.ModuleId != ticket.ModuleId)
            return false;

        if (!string.IsNullOrWhiteSpace(rule.RegionCode)
            && !string.Equals(rule.RegionCode, companyRegion, StringComparison.OrdinalIgnoreCase))
            return false;

        if (rule.Priority.HasValue && rule.Priority != ticket.Priority)
            return false;

        if (rule.TicketType.HasValue && rule.TicketType != ticket.Type)
            return false;

        if (!string.IsNullOrWhiteSpace(rule.Keywords))
        {
            var keywords = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var text = $"{ticket.Title} {ticket.Description}".ToLowerInvariant();
            if (!keywords.Any(k => text.Contains(k.ToLowerInvariant())))
                return false;
        }

        return true;
    }

    private async Task<Guid?> AutoAssignAgent(Guid groupId, CancellationToken ct)
    {
        var groupInfo = await _routingContext.GetGroupRoutingInfoAsync(groupId, ct);
        if (!groupInfo.HasValue) return null;

        // Manual strategy means no auto-assignment
        if (groupInfo.Value.AssignmentStrategy == (int)Ticketing.Domain.Enums.AssignmentStrategy.Manual)
            return null;

        var strategy = _strategyFactory.Resolve(groupInfo.Value.AssignmentStrategy);
        return await strategy.SelectAgentAsync(groupId, ct);
    }
}
