using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketVisibilityService
{
    Task<IQueryable<Ticket>> ApplyVisibilityFilterAsync(IQueryable<Ticket> query, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsInternalAgentAsync(Guid userId, CancellationToken cancellationToken);
}
