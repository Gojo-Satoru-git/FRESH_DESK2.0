using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class AgentBadge : BaseEntity
{
    public Guid AgentId { get; private set; }

    public Guid BadgeId { get; private set; }

    public Guid? TicketId { get; private set; }

    public DateTime EarnedAt { get; private set; }

    public bool Notified { get; private set; }

    public Badge Badge { get; private set; } = null!;
}
