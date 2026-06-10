using Adrenalin.Persistence.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRepository
{
	Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task AddAsync(Ticket ticket, CancellationToken ct = default);
	Task<string?> GetCurrentStatusAsync(Guid id, CancellationToken ct = default);
	Task AddStatusHistoryAsync(TicketStatusHistory history, CancellationToken ct = default);
}