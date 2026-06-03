using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Defines what actions earn points. max_per_day prevents farming: engine checks SUM(points) WHERE agent_id=X AND rule_id=Y AND earned_at::date = TODAY. Negative points allowed for reversals.
/// </summary>
public partial class PointRule
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public int Points { get; set; }

    public int? MaxPerDay { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AgentPoint> AgentPoints { get; set; } = new List<AgentPoint>();

    public virtual User? CreatedByNavigation { get; set; }
}
