using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface IGroupAssignmentHistoryRepository
{
    Task<IReadOnlyList<GroupAssignmentHistory>> GetByTicketAsync(Guid ticketId, CancellationToken ct = default);
    void Add(GroupAssignmentHistory history);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
