using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketCustomField : BaseEntity
{
    public Guid TicketId { get; private set; }

    public string FieldKey { get; private set; } = null!;

    public string? FieldValue { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    private TicketCustomField() { }

    public static TicketCustomField Create(Guid ticketId, string fieldKey, string? fieldValue)
    {
        return new TicketCustomField
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            FieldKey = fieldKey,
            FieldValue = fieldValue,
            CreatedAt = DateTime.UtcNow
        };
    }
}
