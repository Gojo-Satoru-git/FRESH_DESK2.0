using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// One row per allowed edge in a graph. API layer validates every status change against this table. requires_field check: named tickets column must be non-null before transition allowed. auto_trigger=true allows automation engine to fire without human actor.
/// </summary>
public partial class StatusTransition
{
    public Guid Id { get; set; }

    public Guid GraphId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string? RequiresRole { get; set; }

    public string? RequiresField { get; set; }

    public bool AutoTrigger { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual TicketStatusGraph Graph { get; set; } = null!;

    public virtual Role? RequiresRoleNavigation { get; set; }
}
