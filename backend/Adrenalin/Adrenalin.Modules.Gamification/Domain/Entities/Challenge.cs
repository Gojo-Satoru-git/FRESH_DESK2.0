using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class Challenge : BaseEntity
{
    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Metric { get; private set; } = null!;

    public int TargetValue { get; private set; }

    public int BonusPoints { get; private set; }

    public Guid? BadgeId { get; private set; }

    public string Scope { get; private set; } = null!;

    public Guid? GroupId { get; private set; }

    public DateTime StartsAt { get; private set; }

    public DateTime EndsAt { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<AgentChallenge> AgentChallenges { get; private set; } = new List<AgentChallenge>();

    public Badge? Badge { get; private set; }
}
