using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketWatcher : SoftDeleteEntity
{
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? AddedByUserId { get; private set; }
    
    private TicketWatcher() { }
    
    public static TicketWatcher Create(Guid ticketId, Guid userId, Guid? addedByUserId = null)
    {
        return new TicketWatcher
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId,
            AddedByUserId = addedByUserId,
            IsDeleted = false
        };
    }
}
