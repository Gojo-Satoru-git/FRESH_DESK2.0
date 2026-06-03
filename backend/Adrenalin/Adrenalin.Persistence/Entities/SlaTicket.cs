using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Per-ticket SLA clock. One-to-one with tickets. paused_minutes accumulates total pause time from On Hold / Product Roadmap / Pending states. resolution_due_at is extended on resume. Breach flags set by SLA engine daemon / scheduled job.
/// </summary>
public partial class SlaTicket
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid PolicyId { get; set; }

    public DateTime FirstResponseDueAt { get; set; }

    public DateTime? FirstResponseAt { get; set; }

    public DateTime ResolutionDueAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool FirstResponseBreached { get; set; }

    public bool ResolutionBreached { get; set; }

    public int PausedMinutes { get; set; }

    public DateTime? LastPausedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Third SLA stage: follow-up deadline. Typically set when ticket enters pending_customer status — agent must follow up if no customer response within N business hours. Prevents stale pending tickets.
    /// </summary>
    public DateTime? FollowUpDueAt { get; set; }

    public DateTime? FollowUpAt { get; set; }

    /// <summary>
    /// TRUE when follow_up_at IS NULL AND NOW() &gt; follow_up_due_at. Set by SLA engine daemon alongside first_response_breached and resolution_breached.
    /// </summary>
    public bool FollowUpBreached { get; set; }

    public virtual SlaPolicy Policy { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
