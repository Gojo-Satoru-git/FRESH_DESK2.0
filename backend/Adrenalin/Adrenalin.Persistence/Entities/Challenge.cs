using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class Challenge
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Metric { get; set; } = null!;

    public int TargetValue { get; set; }

    public int BonusPoints { get; set; }

    public Guid? BadgeId { get; set; }

    public string Scope { get; set; } = null!;

    public Guid? GroupId { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AgentChallenge> AgentChallenges { get; set; } = new List<AgentChallenge>();

    public virtual Badge? Badge { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Group? Group { get; set; }
}
