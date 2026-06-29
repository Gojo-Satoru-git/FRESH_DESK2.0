using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Notification.Domain.Entities;

public sealed class NotificationLog : BaseEntity
{
    public Guid? CompanyId { get; set; }

    public Guid? TicketId { get; set; }

    public Guid TemplateId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public bool IsFailedDelivery { get; set; }

    public string? TicketNumber { get;  set; }

    public DateTime SentAt { get; set; }

    public NotificationTemplate Template { get; set; } = null!;
}
