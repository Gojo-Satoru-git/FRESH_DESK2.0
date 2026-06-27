// ─────────────────────────────────────────────────────────────────────────────
// NEW FILE (additive — does not touch ISlaRepository.cs).
// Main's SLA module uses one flat ISlaRepository for everything, rather than
// the branch's per-entity repository-interface split. To minimize the merge
// footprint we keep that convention: SlaStageConfig gets its own small
// interface (new entity, no existing consumers to break), and the handful of
// new clock operations (pause/resume/reset/warn) are added as new method
// signatures you append to the existing ISlaRepository interface — see the
// comment block at the bottom of this file for exactly what to paste in.
// ─────────────────────────────────────────────────────────────────────────────

using Adrenalin.Modules.SLA.Domain.Entities;

namespace Adrenalin.Modules.SLA.Domain.Interfaces;

public interface ISlaStageConfigRepository
{
    Task<SlaStageConfig?> GetByStageCodeAsync(string stageCode, CancellationToken ct = default);
    Task<SlaStageConfig?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SlaStageConfig>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SlaStageConfig>> GetAllAsync(CancellationToken ct = default);
    void Add(SlaStageConfig config);
    void Update(SlaStageConfig config);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────
// PASTE INTO existing Adrenalin.Modules.SLA/Domain/Interfaces/ISlaRepository.cs
// — add these signatures inside the existing `public interface ISlaRepository { ... }`,
// underneath the existing MarkFollowUpBreachedAsync method.
// ─────────────────────────────────────────────────────────────────────────────
//
//  // ── v11 additions: tenant-aware lookup + clock lifecycle + warnings ──────
//
//  Task<SlaTicket?> GetByTicketIdAsync(
//      Guid ticketId, string tenantId = "default", CancellationToken ct = default);
//
//  Task PauseClockAsync(
//      Guid ticketId, DateTime pausedAtUtc, string tenantId = "default", CancellationToken ct = default);
//
//  Task ResumeClockAsync(
//      Guid ticketId, DateTime resumedAtUtc, string tenantId = "default", CancellationToken ct = default);
//
//  Task ResetClockAsync(
//      Guid ticketId, Guid newPolicyId, DateTime newFirstResponseDueAt,
//      DateTime newResolutionDueAt, string tenantId = "default", CancellationToken ct = default);
//
//  // Gap 2: 80%-elapsed warning scan — backed by idx_sla_tickets_warn_80pct
//  Task<List<SlaTicket>> GetApproachingEightyPercentAsync(
//      double thresholdPct = 0.80, CancellationToken ct = default);
//
//  // Gap 2: 30-min-before-breach warning scan — backed by idx_sla_tickets_warn_30min_res / _fr
//  Task<List<SlaTicket>> GetApproachingThirtyMinuteBreachAsync(
//      CancellationToken ct = default);
//
//  Task MarkWarnedAt80PctAsync(Guid slaTicketId, CancellationToken ct = default);
//  Task MarkWarnedAt30MinAsync(Guid slaTicketId, CancellationToken ct = default);
//
//  // Gap 1: per-stage timer behaviour (Run/Pause/Reset on entry/exit)
//  Task<SlaStageConfig?> GetStageConfigAsync(string stageCode, CancellationToken ct = default);
//  Task UpsertStageConfigAsync(
//      string stageCode, string stageName, SlaTimerBehaviour behaviour,
//      Guid? overridePolicyId, Guid actorId, CancellationToken ct = default);
