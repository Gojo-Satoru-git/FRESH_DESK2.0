using System;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketTag : BaseEntity
{
    public Guid TicketId { get; private set; }
    public string TagName { get; private set; } = null!;
    public Ticket Ticket { get; private set; } = null!;

    private TicketTag() { }

    public static TicketTag Create(Guid ticketId, string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new TicketDomainException("Tag name cannot be empty.");

        return new TicketTag
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            TagName = tagName.Trim()
        };
    }
}
