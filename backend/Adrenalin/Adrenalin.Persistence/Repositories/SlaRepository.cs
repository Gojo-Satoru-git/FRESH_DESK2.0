using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.SLA.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

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
         .AsNoTracking()
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

        // SlaTicket has private setters — need EF Core direct update
        await _db.SlaTickets
            .Where(s => s.Id == slaTicketId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.FirstResponseBreached, true)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public async Task MarkResolutionBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default)
    {
        await _db.SlaTickets
            .Where(s => s.Id == slaTicketId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ResolutionBreached, true)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public async Task MarkFollowUpBreachedAsync(
        Guid slaTicketId, CancellationToken ct = default)
    {
        await _db.SlaTickets
            .Where(s => s.Id == slaTicketId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.FollowUpBreached, true)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                ct);
    }
}