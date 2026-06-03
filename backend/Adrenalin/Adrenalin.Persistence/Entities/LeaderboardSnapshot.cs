using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Pre-computed daily/weekly/monthly leaderboards. Rebuilt at 00:05 by scheduled job. Reading live SUM on agent_points at request time is too expensive at scale.
/// </summary>
public partial class LeaderboardSnapshot
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public Guid? GroupId { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int TotalPoints { get; set; }

    public int TicketsResolved { get; set; }

    public int SlaMetCount { get; set; }

    public decimal? AvgCsat { get; set; }

    public int? Rank { get; set; }

    public DateTime ComputedAt { get; set; }

    public virtual User Agent { get; set; } = null!;

    public virtual Group? Group { get; set; }
}
