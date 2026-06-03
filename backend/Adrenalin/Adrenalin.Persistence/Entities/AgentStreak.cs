using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// One row per agent. current_streak_days = consecutive days with ≥1 SLA-met resolution. Nightly job resets if last_activity_date &lt; CURRENT_DATE - 1 day.
/// </summary>
public partial class AgentStreak
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public int CurrentStreakDays { get; set; }

    public int LongestStreakDays { get; set; }

    public DateOnly? StreakStartDate { get; set; }

    public DateOnly? LastActivityDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Agent { get; set; } = null!;
}
