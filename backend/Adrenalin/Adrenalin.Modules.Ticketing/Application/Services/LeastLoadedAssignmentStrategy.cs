using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Services;

/// <summary>
/// Wraps the existing <c>ITicketRepository.GetLeastLoadedAgentInGroupAsync</c>
/// as the default auto-assignment strategy.
/// </summary>
public sealed class LeastLoadedAssignmentStrategy : IAgentAssignmentStrategy
{
    private readonly ITicketRepository _ticketRepo;

    public LeastLoadedAssignmentStrategy(ITicketRepository ticketRepo)
        => _ticketRepo = ticketRepo;

    public AssignmentStrategy Strategy => AssignmentStrategy.LeastLoaded;

    public async Task<Guid?> SelectAgentAsync(Guid groupId, CancellationToken ct = default)
        => await _ticketRepo.GetLeastLoadedAgentInGroupAsync(groupId, ct);
}
