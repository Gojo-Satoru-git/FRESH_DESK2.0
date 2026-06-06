using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class AgentChallenge : BaseEntity
{
    public Guid AgentId { get; private set; }

    public Guid ChallengeId { get; private set; }

    public int CurrentValue { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public int PointsAwarded { get; private set; }

    public Challenge Challenge { get; private set; } = null!;
}
