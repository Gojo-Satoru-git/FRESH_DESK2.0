using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Immutable audit log of every status transition. Never update or delete rows. Append-only; enables full status trail and SLA clock reconstruction.
/// </summary>
public partial class TicketStatusHistory
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public Guid? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
