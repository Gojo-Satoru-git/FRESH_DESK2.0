using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class AgentPoint : BaseEntity
{
    public Guid AgentId { get; private set; }

    public Guid RuleId { get; private set; }

    public Guid? TicketId { get; private set; }

    public int Points { get; private set; }

    public string? Description { get; private set; }

    public DateTime EarnedAt { get; private set; }

    public DateOnly EarnedDate { get; private set; }

    public PointRule Rule { get; private set; } = null!;
}
