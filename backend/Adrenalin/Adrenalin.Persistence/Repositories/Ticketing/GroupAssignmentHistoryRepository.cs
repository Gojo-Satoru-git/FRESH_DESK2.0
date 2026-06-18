using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class GroupAssignmentHistoryRepository : IGroupAssignmentHistoryRepository
{
    private readonly AdrenalinDbContext _db;
    public GroupAssignmentHistoryRepository(AdrenalinDbContext db) => _db = db;

    public async Task<IReadOnlyList<GroupAssignmentHistory>> GetByTicketAsync(Guid ticketId, CancellationToken ct = default)
        => await _db.GroupAssignmentHistories
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

    public void Add(GroupAssignmentHistory history) => _db.GroupAssignmentHistories.Add(history);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
