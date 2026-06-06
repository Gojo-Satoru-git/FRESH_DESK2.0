using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class AgentStreak : BaseEntity
{
    public Guid AgentId { get; private set; }

    public int CurrentStreakDays { get; private set; }

    public int LongestStreakDays { get; private set; }

    public DateOnly? StreakStartDate { get; private set; }

    public DateOnly? LastActivityDate { get; private set; }

    public DateTime UpdatedAt { get; private set; }
}
