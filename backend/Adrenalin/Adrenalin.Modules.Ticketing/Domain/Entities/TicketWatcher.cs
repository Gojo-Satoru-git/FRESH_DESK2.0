using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketWatcher : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid AddedBy { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private TicketWatcher() { }
    private TicketWatcher(Guid ticketId, Guid userId, Guid addedBy)
    {
        TicketId = ticketId;
        UserId = userId;
        AddedBy = addedBy;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public static TicketWatcher Create(Guid ticketId, Guid userId, Guid addedBy)
    {
        if (ticketId == Guid.Empty)
            throw new TicketDomainException("Ticket ID cannot be empty.");

        if (userId == Guid.Empty)
            throw new TicketDomainException("User ID cannot be empty.");

        if (addedBy == Guid.Empty)
            throw new TicketDomainException("AddedBy user ID cannot be empty.");

        return new TicketWatcher(ticketId, userId, addedBy);
    }
}