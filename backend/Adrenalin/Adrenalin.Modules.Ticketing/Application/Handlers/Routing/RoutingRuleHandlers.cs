using Adrenalin.Modules.Ticketing.Application.Commands.Routing;
using Adrenalin.Modules.Ticketing.Application.DTOs.Routing;
using Adrenalin.Modules.Ticketing.Application.Queries.Routing;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Routing;

// ═══ CREATE ROUTING RULE ═════════════════════════════════════════════════════

public sealed class CreateRoutingRuleCommandHandler
    : IRequestHandler<CreateRoutingRuleCommand, Result<Guid>>
{
    private readonly IRoutingRuleRepository _rules;
    private readonly ITicketRoutingContextRepository _routingContext;

    public CreateRoutingRuleCommandHandler(
        IRoutingRuleRepository rules,
        ITicketRoutingContextRepository routingContext)
    {
        _rules = rules;
        _routingContext = routingContext;
    }

    public async Task<Result<Guid>> Handle(CreateRoutingRuleCommand cmd, CancellationToken ct)
    {
        try
        {
            var groupExists = await _routingContext.GroupExistsAsync(cmd.GroupId, ct);
            if (!groupExists)
                return Result<Guid>.Failure($"Group {cmd.GroupId} not found or deleted.");

            var existingRules = await _rules.GetByCompanyOrderedAsync(cmd.CompanyId, ct);
            
            if (cmd.IsDefault && existingRules.Any(r => r.IsDefault))
                return Result<Guid>.Failure($"Company {cmd.CompanyId} already has a default routing rule.");

            if (existingRules.Any(r => 
                r.ModuleId == cmd.ModuleId &&
                string.Equals(r.RegionCode, cmd.RegionCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.TierCode, cmd.TierCode, StringComparison.OrdinalIgnoreCase) &&
                r.Priority == cmd.Priority &&
                r.TicketType == cmd.TicketType &&
                string.Equals(r.Keywords, cmd.Keywords, StringComparison.OrdinalIgnoreCase)))
            {
                return Result<Guid>.Failure("An identical routing rule already exists for this company.");
            }

            var rule = CompanyRoutingRule.Create(
                cmd.CompanyId, cmd.GroupId, cmd.ModuleId,
                cmd.RegionCode, cmd.TierCode, cmd.Priority,
                cmd.TicketType, cmd.Keywords, cmd.RulePriority,
                cmd.IsDefault, cmd.ActorId);

            _rules.Add(rule);
            await _rules.SaveChangesAsync(ct);
            return Result<Guid>.Success(rule.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

// ═══ UPDATE ROUTING RULE ═════════════════════════════════════════════════════

public sealed class UpdateRoutingRuleCommandHandler
    : IRequestHandler<UpdateRoutingRuleCommand, Result>
{
    private readonly IRoutingRuleRepository _rules;
    private readonly ITicketRoutingContextRepository _routingContext;

    public UpdateRoutingRuleCommandHandler(
        IRoutingRuleRepository rules,
        ITicketRoutingContextRepository routingContext)
    {
        _rules = rules;
        _routingContext = routingContext;
    }

    public async Task<Result> Handle(UpdateRoutingRuleCommand cmd, CancellationToken ct)
    {
        try
        {
            var rule = await _rules.GetByIdAsync(cmd.RuleId, ct);
            if (rule is null)
                return Result.Failure($"Routing rule {cmd.RuleId} not found.");

            var groupExists = await _routingContext.GroupExistsAsync(cmd.GroupId, ct);
            if (!groupExists)
                return Result.Failure($"Group {cmd.GroupId} not found or deleted.");

            var existingRules = await _rules.GetByCompanyOrderedAsync(rule.CompanyId, ct);

            if (cmd.IsDefault && !rule.IsDefault && existingRules.Any(r => r.IsDefault && r.Id != rule.Id))
                return Result.Failure($"Company {rule.CompanyId} already has a default routing rule.");

            if (existingRules.Any(r => 
                r.Id != rule.Id &&
                r.ModuleId == cmd.ModuleId &&
                string.Equals(r.RegionCode, cmd.RegionCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.TierCode, cmd.TierCode, StringComparison.OrdinalIgnoreCase) &&
                r.Priority == cmd.Priority &&
                r.TicketType == cmd.TicketType &&
                string.Equals(r.Keywords, cmd.Keywords, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure("An identical routing rule already exists for this company.");
            }

            rule.Update(
                cmd.GroupId, cmd.ModuleId, cmd.RegionCode,
                cmd.TierCode, cmd.Priority, cmd.TicketType,
                cmd.Keywords, cmd.RulePriority, cmd.IsDefault,
                cmd.ActorId);

            _rules.Update(rule);
            await _rules.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ DELETE ROUTING RULE ═════════════════════════════════════════════════════

public sealed class DeleteRoutingRuleCommandHandler
    : IRequestHandler<DeleteRoutingRuleCommand, Result>
{
    private readonly IRoutingRuleRepository _rules;
    private readonly ITicketRoutingContextRepository _routingContext;

    public DeleteRoutingRuleCommandHandler(IRoutingRuleRepository rules, ITicketRoutingContextRepository routingContext)
    {
        _rules = rules;
        _routingContext = routingContext;
    }

    public async Task<Result> Handle(DeleteRoutingRuleCommand cmd, CancellationToken ct)
    {
        try
        {
            var rule = await _rules.GetByIdAsync(cmd.RuleId, ct);
            if (rule is null)
                return Result.Failure($"Routing rule {cmd.RuleId} not found.");

            var existingRules = await _rules.GetByCompanyOrderedAsync(rule.CompanyId, ct);
            if (existingRules.Count == 1)
            {
                var defaultGroup = await _routingContext.GetCompanyDefaultGroupAsync(rule.CompanyId, ct);
                if (!defaultGroup.HasValue)
                    return Result.Failure("Cannot delete the last routing rule because the company has no default group mapping. A company must always have at least one routing path.");
            }

            rule.SoftDelete(cmd.ActorId);
            _rules.Update(rule);
            await _rules.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ QUERY: GET ROUTING RULES ════════════════════════════════════════════════

public sealed class GetRoutingRulesQueryHandler
    : IRequestHandler<GetRoutingRulesQuery, Result<IReadOnlyList<RoutingRuleDto>>>
{
    private readonly IRoutingRuleRepository _rules;
    private readonly ITicketRoutingContextRepository _routingContext;

    public GetRoutingRulesQueryHandler(
        IRoutingRuleRepository rules,
        ITicketRoutingContextRepository routingContext)
    {
        _rules = rules;
        _routingContext = routingContext;
    }

    public async Task<Result<IReadOnlyList<RoutingRuleDto>>> Handle(
        GetRoutingRulesQuery query, CancellationToken ct)
    {
        try
        {
            var rules = query.CompanyId.HasValue
                ? await _rules.GetByCompanyOrderedAsync(query.CompanyId.Value, ct)
                : await _rules.GetAllAsync(ct);

            var groupIds = rules.Select(r => r.GroupId).ToList();
            var groupLookup = await _routingContext.GetGroupNamesAsync(groupIds, ct);

            var dtos = rules.Select(r => new RoutingRuleDto(
                r.Id, r.CompanyId, "—", // Company name resolved by caller/controller if needed
                r.GroupId, groupLookup.GetValueOrDefault(r.GroupId, "Unknown"),
                r.ModuleId, null, // Module name resolved if needed
                r.RegionCode, r.TierCode, r.Priority, r.TicketType,
                r.Keywords, r.RulePriority, r.IsDefault,
                r.CreatedAt, r.UpdatedAt
            )).ToList();

            return Result<IReadOnlyList<RoutingRuleDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<RoutingRuleDto>>.Failure(ex.Message); }
    }
}

// ═══ QUERY: GET ROUTING RULE BY ID ═══════════════════════════════════════════

public sealed class GetRoutingRuleByIdQueryHandler
    : IRequestHandler<GetRoutingRuleByIdQuery, Result<RoutingRuleDto>>
{
    private readonly IRoutingRuleRepository _rules;
    private readonly ITicketRoutingContextRepository _routingContext;

    public GetRoutingRuleByIdQueryHandler(
        IRoutingRuleRepository rules,
        ITicketRoutingContextRepository routingContext)
    {
        _rules = rules;
        _routingContext = routingContext;
    }

    public async Task<Result<RoutingRuleDto>> Handle(
        GetRoutingRuleByIdQuery query, CancellationToken ct)
    {
        try
        {
            var rule = await _rules.GetByIdAsync(query.RuleId, ct);
            if (rule is null)
                return Result<RoutingRuleDto>.Failure($"Routing rule {query.RuleId} not found.");

            var groupName = await _routingContext.GetGroupNameAsync(rule.GroupId, ct);

            var dto = new RoutingRuleDto(
                rule.Id, rule.CompanyId, "—",
                rule.GroupId, groupName ?? "Unknown",
                rule.ModuleId, null,
                rule.RegionCode, rule.TierCode, rule.Priority, rule.TicketType,
                rule.Keywords, rule.RulePriority, rule.IsDefault,
                rule.CreatedAt, rule.UpdatedAt);

            return Result<RoutingRuleDto>.Success(dto);
        }
        catch (Exception ex) { return Result<RoutingRuleDto>.Failure(ex.Message); }
    }
}

// ═══ SIMULATE ROUTING RULE ═══════════════════════════════════════════════════

public sealed class SimulateRoutingCommandHandler
    : IRequestHandler<SimulateRoutingCommand, Result<RoutingSimulationResultDto>>
{
    private readonly ITicketRoutingEngine _routingEngine;

    public SimulateRoutingCommandHandler(ITicketRoutingEngine routingEngine)
    {
        _routingEngine = routingEngine;
    }

    public async Task<Result<RoutingSimulationResultDto>> Handle(
        SimulateRoutingCommand cmd, CancellationToken ct)
    {
        try
        {
            var ticket = Ticket.Create(
                cmd.CompanyId,
                cmd.ModuleId ?? Guid.NewGuid(), // Dummy module ID if not provided, since entity requires it
                cmd.Title ?? "Simulated Ticket",
                cmd.Description ?? "Simulation Payload",
                cmd.Type ?? Adrenalin.Modules.Ticketing.Domain.Enums.TicketType.Incident,
                Adrenalin.Modules.Ticketing.Domain.Enums.TicketSource.Portal,
                null,
                cmd.Priority ?? Adrenalin.Modules.Ticketing.Domain.Enums.TicketPriority.Medium);

            var result = await _routingEngine.SimulateAsync(ticket, ct);
            return Result<RoutingSimulationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<RoutingSimulationResultDto>.Failure(ex.Message);
        }
    }
}

// ═══ QUERY: GET ROUTING RULE HISTORY ═════════════════════════════════════════

public sealed class GetRoutingRuleHistoryQueryHandler
    : IRequestHandler<GetRoutingRuleHistoryQuery, Result<IReadOnlyList<RoutingRuleHistoryDto>>>
{
    private readonly ITicketRoutingContextRepository _routingContext;

    public GetRoutingRuleHistoryQueryHandler(ITicketRoutingContextRepository routingContext)
    {
        _routingContext = routingContext;
    }

    public async Task<Result<IReadOnlyList<RoutingRuleHistoryDto>>> Handle(
        GetRoutingRuleHistoryQuery query, CancellationToken ct)
    {
        try
        {
            var history = await _routingContext.GetRoutingRuleHistoryAsync(query.RuleId, ct);
            return Result<IReadOnlyList<RoutingRuleHistoryDto>>.Success(history);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<RoutingRuleHistoryDto>>.Failure(ex.Message);
        }
    }
}
