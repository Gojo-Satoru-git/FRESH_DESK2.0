using Adrenalin.Modules.SLA.Domain.Entities;

namespace Adrenalin.Modules.SLA.Domain.Interfaces;

public interface ISlaRepository
{
    // escalation_rules table
    Task<List<EscalationRule>> GetActiveEscalationRulesAsync(
        CancellationToken ct = default);

    // sla_tickets table — tickets overdue
    Task<List<SlaTicket>> GetOverdueSlaTicketsAsync(
        CancellationToken ct = default);

    // ticket.tickets — get group_id
    Task<Guid?> GetTicketGroupIdAsync(
        Guid ticketId, CancellationToken ct = default);

    // auth — get users by role in group
    Task<List<Guid>> GetUserIdsByRoleInGroupAsync(
        Guid groupId, string role,
        CancellationToken ct = default);

    Task<string> GetTicketNumberAsync(Guid ticketId, CancellationToken ct);

    // Mark breaches on sla_tickets
    Task MarkFirstResponseBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default);

    Task MarkResolutionBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default);

    Task MarkFollowUpBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default);
}