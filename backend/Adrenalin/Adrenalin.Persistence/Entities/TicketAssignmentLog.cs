using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class TicketAssignmentLog
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? FromAgentId { get; set; }

    public Guid ToAgentId { get; set; }

    public Guid? ChangedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public string? Notes { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual User? FromAgent { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User ToAgent { get; set; } = null!;
}
