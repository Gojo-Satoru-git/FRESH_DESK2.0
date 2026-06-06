using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class LeaderboardSnapshot : BaseEntity
{
    public Guid AgentId { get; private set; }

    public Guid? GroupId { get; private set; }

    public DateOnly PeriodStart { get; private set; }

    public DateOnly PeriodEnd { get; private set; }

    public int TotalPoints { get; private set; }

    public int TicketsResolved { get; private set; }

    public int SlaMetCount { get; private set; }

    public decimal? AvgCsat { get; private set; }

    public int? Rank { get; private set; }

    public DateTime ComputedAt { get; private set; }
}
