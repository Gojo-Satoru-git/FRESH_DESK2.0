using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Modules.Ticketing.Application.Services;

/// <summary>
/// Placeholder for the "proper" auto-assignment engine: role eligibility gate,
/// then hard-factor filters (region/skill/language/shift/capacity), then
/// weighted soft-factor scoring among the survivors.
///
/// THIS IS DELIBERATELY NOT IMPLEMENTED YET.
///
/// The data this strategy needs — workflow stages, factor masters, agent
/// factor values, stage factor policies — is owned by the Workflow module,
/// which is being built by a different developer in a separate branch and
/// does not exist in this codebase yet.
///
/// Rather than fake a result (e.g. silently falling back to LeastLoaded,
/// which would hide the gap and could mis-route tickets that genuinely need
/// skill/region/language matching), this strategy always returns null.
/// A null result is treated by <see cref="TicketRoutingEngine"/> exactly like
/// "no agent available" — the ticket keeps its GroupId, AssignedAgentId stays
/// null, and it lands in that group's queue for a lead/dispatcher to pick up
/// manually (see GetGroupQueueQueryHandler / GroupQueueController).
///
/// TO ACTIVATE THE REAL ENGINE LATER:
/// Replace the body of SelectAgentAsync below with the real role/hard-filter/
/// scoring pipeline. Nothing else needs to change — AgentAssignmentStrategyFactory
/// already resolves this strategy whenever Group.AssignmentStrategy == 4
/// (AssignmentStrategy.FactorBased), and TicketRoutingEngine already treats a
/// null return as "queue it" today, so the queuing behaviour doesn't change
/// either way — only whether the queue is empty or full.
/// </summary>
public sealed class FactorBasedAssignmentStrategy : IAgentAssignmentStrategy
{
    private readonly ILogger<FactorBasedAssignmentStrategy> _log;

    public FactorBasedAssignmentStrategy(ILogger<FactorBasedAssignmentStrategy> log)
        => _log = log;

    public AssignmentStrategy Strategy => AssignmentStrategy.FactorBased;

    public Task<Guid?> SelectAgentAsync(Guid groupId, CancellationToken ct = default)
    {
        // INTENTIONAL NO-OP / FAILURE.
        // Do not change this to call GetLeastLoadedAgentInGroupAsync or any
        // other fallback here — that would silently mask the missing factor
        // engine. Let it fail; the routing engine's queuing path is what
        // should pick up the slack until the Workflow module ships.
        _log.LogInformation(
            "FactorBased assignment requested for group {GroupId} but the " +
            "role/hard-filter/scoring engine is not implemented yet (Workflow " +
            "module pending). Ticket will remain unassigned in the group queue.",
            groupId);

        return Task.FromResult<Guid?>(null);
    }
}
