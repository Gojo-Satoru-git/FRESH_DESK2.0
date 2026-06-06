using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Notification.Domain.Entities;

public sealed class NotificationLog : BaseEntity
{
    public Guid? TicketId { get; private set; }

    public Guid TemplateId { get; private set; }

    public string RecipientEmail { get; private set; } = null!;

    public string? ErrorMessage { get; private set; }

    public bool IsFailedDelivery { get; private set; }

    public DateTime SentAt { get; private set; }

    public NotificationTemplate Template { get; private set; } = null!;
}
