using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Handlebars templates for all notification events. code is the stable reference key. Examples: TICKET_CREATED, AGENT_REPLY, CSAT_SURVEY, SLA_BREACH, BADGE_AWARDED.
/// </summary>
public partial class NotificationTemplate
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Subject { get; set; }

    public string? BodyHtml { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();

    public virtual User? UpdatedByNavigation { get; set; }
}
