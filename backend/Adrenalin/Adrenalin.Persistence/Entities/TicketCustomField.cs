using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class TicketCustomField
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string FieldKey { get; set; } = null!;

    public string? FieldValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
