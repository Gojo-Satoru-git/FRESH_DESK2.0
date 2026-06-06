using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Notification.Domain.Entities;

public sealed class NotificationTemplate : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Subject { get; private set; }

    public string? BodyHtml { get; private set; }

    public ICollection<NotificationLog> NotificationLogs { get; private set; } = new List<NotificationLog>();
}
