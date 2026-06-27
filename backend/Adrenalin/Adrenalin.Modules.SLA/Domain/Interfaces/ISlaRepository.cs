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

    // ── v11 additions: tenant-aware lookup + clock lifecycle + warnings ──────
    // (these were already implemented in SlaRepository.cs — this just declares
    // them on the interface so callers like SlaStageHandlers.cs can see them)

    Task<SlaTicket?> GetByTicketIdAsync(
        Guid ticketId, string tenantId = "default", CancellationToken ct = default);

    Task PauseClockAsync(
        Guid ticketId, DateTime pausedAtUtc, string tenantId = "default", CancellationToken ct = default);

    Task ResumeClockAsync(
        Guid ticketId, DateTime resumedAtUtc, string tenantId = "default", CancellationToken ct = default);

    Task ResetClockAsync(
        Guid ticketId, Guid newPolicyId, DateTime newFirstResponseDueAt,
        DateTime newResolutionDueAt, string tenantId = "default", CancellationToken ct = default);

    Task<List<SlaTicket>> GetApproachingEightyPercentAsync(
        double thresholdPct = 0.80, CancellationToken ct = default);

    Task<List<SlaTicket>> GetApproachingThirtyMinuteBreachAsync(
        CancellationToken ct = default);

    Task MarkWarnedAt80PctAsync(Guid slaTicketId, CancellationToken ct = default);
    Task MarkWarnedAt30MinAsync(Guid slaTicketId, CancellationToken ct = default);

    Task<SlaStageConfig?> GetStageConfigAsync(string stageCode, CancellationToken ct = default);

    Task UpsertStageConfigAsync(
        string stageCode, string stageName, Adrenalin.SharedKernel.Enums.SLA.SlaTimerBehaviour behaviour,
        Guid? overridePolicyId, Guid actorId, CancellationToken ct = default);
}