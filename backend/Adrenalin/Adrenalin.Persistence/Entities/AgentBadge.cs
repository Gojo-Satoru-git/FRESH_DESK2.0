using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class AgentBadge
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public Guid BadgeId { get; set; }

    public Guid? TicketId { get; set; }

    public DateTime EarnedAt { get; set; }

    public bool Notified { get; set; }

    public virtual User Agent { get; set; } = null!;

    public virtual Badge Badge { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}
