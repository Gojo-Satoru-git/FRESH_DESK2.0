using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class SlaTicket : AuditableEntity
{
    public Guid TicketId { get; private set; }

    public Guid PolicyId { get; private set; }

    public DateTime FirstResponseDueAt { get; private set; }

    public DateTime? FirstResponseAt { get; private set; }

    public DateTime ResolutionDueAt { get; private set; }

    public DateTime? ResolvedAt { get; private set; }

    public bool FirstResponseBreached { get; private set; }

    public bool ResolutionBreached { get; private set; }

    public int PausedMinutes { get; private set; }

    public DateTime? LastPausedAt { get; private set; }

    public DateTime? FollowUpDueAt { get; private set; }

    public DateTime? FollowUpAt { get; private set; }

    public bool FollowUpBreached { get; private set; }

    public SlaPolicy Policy { get; private set; } = null!;
}
