// MERGED: main's SlaTicket.cs (kept all existing properties/behaviour) +
// your branch's additions (TenantId, WarnedAt80Pct, WarnedAt30Min, Pause/Resume,
// ResetClock). Matches schema_sla_v11_postgres.sql columns exactly:
//   tenant_id, warned_at_80_pct, warned_at_30_min (new in v11)
// Nothing that existed in main was removed.

using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class SlaTicket : AuditableEntity
{
    public Guid TicketId { get; private set; }

    public Guid PolicyId { get; private set; }

    /// <summary>Gap 4 (v11): tenant isolation. "default" for single-tenant deployments.</summary>
    public string TenantId { get; private set; } = "default";

    public DateTime FirstResponseDueAt { get; private set; }

    public DateTime? FirstResponseAt { get; private set; }

    public DateTime ResolutionDueAt { get; private set; }

    public DateTime? ResolvedAt { get; private set; }

    public bool FirstResponseBreached { get; private set; }

    public bool ResolutionBreached { get; private set; }

    public int PausedMinutes { get; private set; }

    public DateTime? LastPausedAt { get; private set; }

    public DateTime? FollowUpDueAt { get; private set; }

    public DateTime? FollowUpAt { get; private set; }

    public bool FollowUpBreached { get; private set; }

    /// <summary>Gap 2 (v11): TRUE once the 80%-elapsed warning has been published. Prevents re-publishing.</summary>
    public bool WarnedAt80Pct { get; private set; }

    /// <summary>Gap 2 (v11): TRUE once the 30-min-before-breach warning has been published.</summary>
    public bool WarnedAt30Min { get; private set; }

    public bool IsCurrentlyPaused => LastPausedAt.HasValue;

    public SlaPolicy Policy { get; private set; } = null!;

    // ── Factory ──────────────────────────────────────────────────────────────

    public static SlaTicket Create(
        Guid ticketId, string tenantId, Guid policyId,
        DateTime firstResponseDueAt, DateTime resolutionDueAt)
    {
        if (ticketId == Guid.Empty) throw new ArgumentException("TicketId cannot be empty.");
        if (policyId == Guid.Empty) throw new ArgumentException("PolicyId cannot be empty.");
        return new SlaTicket
        {
            TicketId = ticketId,
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? "default" : tenantId,
            PolicyId = policyId,
            FirstResponseDueAt = firstResponseDueAt,
            ResolutionDueAt = resolutionDueAt,
            PausedMinutes = 0,
            WarnedAt80Pct = false,
            WarnedAt30Min = false,
        };
    }

    // ── Existing main behaviour (unchanged) ───────────────────────────────────

    public void MarkFirstResponseBreached() => FirstResponseBreached = true;

    public void MarkResolutionBreached() => ResolutionBreached = true;

    public void MarkFollowUpBreached() => FollowUpBreached = true;

    // ── Ported from branch ────────────────────────────────────────────────────

    public void RecordFirstResponse(DateTime respondedAt)
    {
        if (FirstResponseAt.HasValue) return;
        FirstResponseAt = respondedAt;
        FirstResponseBreached = respondedAt > FirstResponseDueAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gap 1: pause the clock on entering a Pause-behaviour stage.</summary>
    public void Pause(DateTime pausedAt)
    {
        if (IsCurrentlyPaused) return;
        if (ResolvedAt.HasValue)
            throw new InvalidOperationException("Cannot pause SLA clock on a resolved ticket.");
        LastPausedAt = pausedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gap 1: resume the clock on leaving a Pause-behaviour stage; extends ResolutionDueAt by the pause duration.</summary>
    public void Resume(DateTime resumedAt)
    {
        if (!IsCurrentlyPaused) return;
        var elapsed = (int)Math.Ceiling((resumedAt - LastPausedAt!.Value).TotalMinutes);
        PausedMinutes += elapsed;
        ResolutionDueAt = ResolutionDueAt.AddMinutes(elapsed);
        LastPausedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordResolution(DateTime resolvedAt)
    {
        if (ResolvedAt.HasValue) return;
        if (IsCurrentlyPaused) Resume(resolvedAt);
        ResolvedAt = resolvedAt;
        ResolutionBreached = resolvedAt > ResolutionDueAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetFollowUpDeadline(DateTime followUpDueAt)
    {
        FollowUpDueAt = followUpDueAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordFollowUp(DateTime followedUpAt)
    {
        if (!FollowUpDueAt.HasValue) return;
        if (FollowUpAt.HasValue) return;
        FollowUpAt = followedUpAt;
        FollowUpBreached = followedUpAt > FollowUpDueAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gap 2: mark the 80%-elapsed warning as published (idempotent — daemon polls every 60s).</summary>
    public void MarkWarnedAt80Pct()
    {
        if (WarnedAt80Pct) return;
        WarnedAt80Pct = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gap 2: mark the 30-min-before-breach warning as published.</summary>
    public void MarkWarnedAt30Min()
    {
        if (WarnedAt30Min) return;
        WarnedAt30Min = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gap 1: used by a Reset-behaviour stage — starts a brand-new SLA window.</summary>
    public void ResetClock(Guid policyId, DateTime newFirstResponseDueAt, DateTime newResolutionDueAt)
    {
        PolicyId = policyId;
        FirstResponseDueAt = newFirstResponseDueAt;
        ResolutionDueAt = newResolutionDueAt;
        FirstResponseAt = null;
        ResolvedAt = null;
        FirstResponseBreached = false;
        ResolutionBreached = false;
        PausedMinutes = 0;
        LastPausedAt = null;
        WarnedAt80Pct = false;
        WarnedAt30Min = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
