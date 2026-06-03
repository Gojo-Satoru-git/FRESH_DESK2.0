using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class Badge
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public int PointsValue { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AgentBadge> AgentBadges { get; set; } = new List<AgentBadge>();

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual User? CreatedByNavigation { get; set; }
}
