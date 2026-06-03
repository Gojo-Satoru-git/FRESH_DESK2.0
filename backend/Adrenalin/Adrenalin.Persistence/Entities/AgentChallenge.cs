using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class AgentChallenge
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public Guid ChallengeId { get; set; }

    public int CurrentValue { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int PointsAwarded { get; set; }

    public virtual User Agent { get; set; } = null!;

    public virtual Challenge Challenge { get; set; } = null!;
}
