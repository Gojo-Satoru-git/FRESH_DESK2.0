using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken);

    Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default);
    
    Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    void Update(Ticket ticket);

    void Remove(Ticket ticket);
}