using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

/// <summary>
/// Resolves the target group for a ticket using the enterprise routing cascade:
/// 1. Explicit Company Routing Rules (priority-ordered)
/// 2. Category (ModuleId) Routing
/// 3. Region Routing (company GeoRegion → group RegionCode)
/// 4. Fallback (company default group → system fallback group)
/// </summary>
public interface ITicketRoutingEngine
{
    Task<RoutingResult> RouteAsync(Ticket ticket, CancellationToken ct = default);
    Task<RoutingSimulationResultDto> SimulateAsync(Ticket ticket, CancellationToken ct = default);
}

/// <summary>
/// Result of the routing engine evaluation.
/// </summary>
/// <param name="GroupId">The resolved target group, or null if no route found.</param>
/// <param name="AgentId">Optionally resolved agent via auto-assignment, or null.</param>
/// <param name="MatchedRule">Describes which cascade level matched: "CompanyExplicit", "CategoryMatch", "RegionMatch", "Fallback", "None".</param>
/// <param name="RuleDescription">Human-readable description of the matched rule.</param>
public sealed record RoutingResult(
    Guid? GroupId,
    Guid? AgentId,
    string MatchedRule,
    string? RuleDescription);
