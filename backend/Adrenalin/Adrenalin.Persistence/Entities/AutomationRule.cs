using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// JSONB conditions/actions allows rule engine to evolve without schema migrations. Examples: route to group on creation, reopen on reply, auto-close at day 15, escalate on no-response, set payment hold status. execution_order determines eval sequence.
/// </summary>
public partial class AutomationRule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Conditions { get; set; } = null!;

    public string Actions { get; set; } = null!;

    public int ExecutionOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AutomationExecutionLog> AutomationExecutionLogs { get; set; } = new List<AutomationExecutionLog>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
