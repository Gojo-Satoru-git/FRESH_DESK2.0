using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class NotificationLog
{
    public Guid Id { get; set; }

    public Guid? TicketId { get; set; }

    public Guid TemplateId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public bool IsFailedDelivery { get; set; }

    public DateTime SentAt { get; set; }

    public virtual NotificationTemplate Template { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}
