using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Immutable append-only point ledger. Event-sourcing pattern. Total = SUM(points). Enables full audit, point reversals (insert negative row), time-travel queries. Never update or delete — insert only.
/// </summary>
public partial class AgentPoint
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public Guid RuleId { get; set; }

    public Guid? TicketId { get; set; }

    public int Points { get; set; }

    public string? Description { get; set; }

    public DateTime EarnedAt { get; set; }

    public DateOnly EarnedDate { get; set; }

    public virtual User Agent { get; set; } = null!;

    public virtual PointRule Rule { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}
