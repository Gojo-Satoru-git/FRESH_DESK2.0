using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.SLA.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Adrenalin.SharedKernel.Enums.SLA;

namespace Adrenalin.Persistence.Repositories.SLA;
public class SlaRepository : ISlaRepository
{
    private readonly AdrenalinDbContext _db;

    public SlaRepository(AdrenalinDbContext db) => _db = db;

    public async Task<List<EscalationRule>> GetActiveEscalationRulesAsync(
        CancellationToken ct = default)
    {
        return await _db.EscalationRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.NoResponseMinutes)
            .ToListAsync(ct);
    }

    public async Task<string> GetTicketNumberAsync(Guid ticketId, CancellationToken ct)
    {
        // Reaches straight into the core tickets table to pull the physical column string
        return await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => t.TicketNumber)
            .FirstOrDefaultAsync(ct) ?? "Unknown";
    }

    public async Task<List<SlaTicket>> GetOverdueSlaTicketsAsync(
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _db.SlaTickets
        .Where(s =>

               s.ResolvedAt == null

            &&
            // First response overdue AND not yet marked breached
            (s.FirstResponseAt == null
                && s.FirstResponseDueAt < now
                && !s.FirstResponseBreached)   // ← KEY FIX
            ||
            // Resolution overdue AND not yet marked breached
            (s.ResolvedAt == null
                && s.ResolutionDueAt < now
                && !s.ResolutionBreached)       // ← KEY FIX
            ||
            // Follow-up overdue AND not yet marked breached
            (s.FollowUpAt == null
                && s.FollowUpDueAt != null
                && s.FollowUpDueAt < now
                && !s.FollowUpBreached))        // ← KEY FIX
        .ToListAsync(ct);
    }

    public async Task<Guid?> GetTicketGroupIdAsync(
        Guid ticketId, CancellationToken ct = default)
    {
        return await _db.Tickets
            .Where(t => t.Id == ticketId && !t.IsDeleted)
            .Select(t => t.GroupId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Guid>> GetUserIdsByRoleInGroupAsync(
        Guid groupId, string role, CancellationToken ct = default)
    {
        return await (
            from ug in _db.UserGroups
            join ur in _db.UserRoles on ug.UserId equals ur.UserId
            join r in _db.Roles on ur.RoleId equals r.Id
            where ug.GroupId == groupId
                && r.Name.ToLower() == role.ToLower()
                && !ug.IsDeleted
                && !ur.IsDeleted
                && !r.IsDeleted
            select ug.UserId
        ).Distinct().ToListAsync(ct);
    }

    public async Task MarkFirstResponseBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default)
    {
        var slaTicket = await _db.SlaTickets
            .FirstOrDefaultAsync(s => s.Id == slaTicketId, ct);

        if (slaTicket is null) return;

        slaTicket.MarkFirstResponseBreached();
    }

    public async Task MarkResolutionBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default)
    {
        var slaTicket = await _db.SlaTickets
            .FirstOrDefaultAsync(s => s.Id == slaTicketId, ct);

        if (slaTicket is null) return;

        slaTicket.MarkResolutionBreached();
    }

    public async Task MarkFollowUpBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default)
    {
        var slaTicket = await _db.SlaTickets
            .FirstOrDefaultAsync(s => s.Id == slaTicketId, ct);

        if (slaTicket is null) return;

        slaTicket.MarkFollowUpBreached();
    }

    public async Task<SlaTicket?> GetByTicketIdAsync(
        Guid ticketId, string tenantId = "default", CancellationToken ct = default)
    {
        return await _db.SlaTickets
            .FirstOrDefaultAsync(s => s.TicketId == ticketId && s.TenantId == tenantId, ct);
    }

    public async Task PauseClockAsync(
        Guid ticketId, DateTime pausedAtUtc, string tenantId = "default", CancellationToken ct = default)
    {
        var slaTicket = await GetByTicketIdAsync(ticketId, tenantId, ct);
        slaTicket?.Pause(pausedAtUtc);
    }

    public async Task ResumeClockAsync(
        Guid ticketId, DateTime resumedAtUtc, string tenantId = "default", CancellationToken ct = default)
    {
        var slaTicket = await GetByTicketIdAsync(ticketId, tenantId, ct);
        slaTicket?.Resume(resumedAtUtc);
    }

    public async Task ResetClockAsync(
        Guid ticketId, Guid newPolicyId, DateTime newFirstResponseDueAt,
        DateTime newResolutionDueAt, string tenantId = "default", CancellationToken ct = default)
    {
        var slaTicket = await GetByTicketIdAsync(ticketId, tenantId, ct);
        slaTicket?.ResetClock(newPolicyId, newFirstResponseDueAt, newResolutionDueAt);
    }

// Gap 2: backed by idx_sla_tickets_warn_80pct — open, not yet breached, not yet warned.
// Elapsed % is computed in memory rather than in SQL since "elapsed" depends on
// CreatedAt→ResolutionDueAt span, which Postgres can express but EF's translator
    // handles more reliably as a post-filter on an already-narrow indexed set.
    public async Task<List<SlaTicket>> GetApproachingEightyPercentAsync(
        double thresholdPct = 0.80, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var candidates = await _db.SlaTickets
            .Where(s =>
                s.ResolvedAt == null &&
                !s.ResolutionBreached &&
                !s.WarnedAt80Pct)
            .ToListAsync(ct);

        return candidates.Where(s =>
        {
            var totalWindow = (s.ResolutionDueAt - s.CreatedAt.UtcDateTime).TotalMinutes;
            if (totalWindow <= 0) return false;
            var elapsed = (now - s.CreatedAt.UtcDateTime).TotalMinutes;
            return elapsed / totalWindow >= thresholdPct;
        }).ToList();
    }

// Gap 2: backed by idx_sla_tickets_warn_30min_res / _fr.
    public async Task<List<SlaTicket>> GetApproachingThirtyMinuteBreachAsync(
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var window = now.AddMinutes(30);

        return await _db.SlaTickets
            .Where(s =>
                !s.WarnedAt30Min &&
                ((s.ResolvedAt == null && !s.ResolutionBreached &&
                s.ResolutionDueAt > now && s.ResolutionDueAt <= window)
                ||
                (s.FirstResponseAt == null && !s.FirstResponseBreached &&
                s.FirstResponseDueAt > now && s.FirstResponseDueAt <= window)))
            .ToListAsync(ct);
    }

    public async Task MarkWarnedAt80PctAsync(Guid slaTicketId, CancellationToken ct = default)
    {
        var slaTicket = await _db.SlaTickets.FirstOrDefaultAsync(s => s.Id == slaTicketId, ct);
        slaTicket?.MarkWarnedAt80Pct();
    }

    public async Task MarkWarnedAt30MinAsync(Guid slaTicketId, CancellationToken ct = default)
    {
        var slaTicket = await _db.SlaTickets.FirstOrDefaultAsync(s => s.Id == slaTicketId, ct);
        slaTicket?.MarkWarnedAt30Min();
    }

    public async Task<SlaStageConfig?> GetStageConfigAsync(string stageCode, CancellationToken ct = default)
    {
        return await _db.SlaStageConfigs.AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.StageCode == stageCode.ToLowerInvariant() && s.IsActive && !s.IsDeleted, ct);
    }

    public async Task UpsertStageConfigAsync(
        string stageCode, string stageName, SlaTimerBehaviour behaviour,
        Guid? overridePolicyId, Guid actorId, CancellationToken ct = default)
    {
        var existing = await _db.SlaStageConfigs
            .FirstOrDefaultAsync(s => s.StageCode == stageCode.ToLowerInvariant(), ct);

        if (existing is null)
        {
            _db.SlaStageConfigs.Add(
                SlaStageConfig.Create(stageCode, stageName, behaviour, overridePolicyId, actorId));
        }
        else
        {
            existing.Update(stageName, behaviour, overridePolicyId, actorId);
        }
    }

}