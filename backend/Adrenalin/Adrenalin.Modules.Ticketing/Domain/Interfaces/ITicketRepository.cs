using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRepository
{
    // ── existing ───────────────────────────────────────────────
    Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task AddAsync(
        Ticket ticket,
        CancellationToken ct = default);

    Task UpdateAssignmentAsync(
        Guid ticketId,
        Guid? agentId,
        Guid? groupId,
        Guid triggeredBy,
        CancellationToken ct = default);

    Task<Guid?> GetLeastLoadedAgentInGroupAsync(
        Guid groupId,
        CancellationToken ct = default);

    // ── ADD THESE — restored missing methods ───────────────────
    Task<Guid?> GetUserCompanyIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Ticket>> GetTicketsAsync(
         string? ticketNumber,
         TicketStatus? status,
         Guid? assignedAgentId,
         Guid? companyId,
         int page,
         int pageSize,
         CancellationToken ct = default);

    Task<int> CountTicketsAsync(
        string? ticketNumber,
        TicketStatus? status,
        Guid? assignedAgentId,
        Guid? companyId,
        CancellationToken ct = default);
}