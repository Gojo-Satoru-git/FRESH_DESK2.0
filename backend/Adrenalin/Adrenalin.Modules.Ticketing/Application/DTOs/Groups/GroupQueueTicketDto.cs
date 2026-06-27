namespace Adrenalin.Modules.Ticketing.Application.DTOs.Groups;

/// <summary>
/// One row in a group's dispatch queue — a ticket that has a Group but no
/// individual AssignedAgentId yet. This is the "fallback Tier 1" state: either
/// FactorBasedAssignmentStrategy returned null (stub, by design), or the group's
/// AssignmentStrategy is Manual, or LeastLoaded found nobody available.
///
/// TicketType and Priority are surfaced explicitly here (not buried in a generic
/// TicketDto) because dispatchers triaging a queue need them front-and-center —
/// this is exactly the "ticket type, priority" exposure the frontend needs.
/// </summary>
public sealed record GroupQueueTicketDto(
    Guid TicketId,
    string? TicketNumber,
    Guid GroupId,
    string GroupName,
    string TicketType,       // Ticket.Type enum, exposed as string for the frontend
    string Priority,         // Ticket.Priority enum, exposed as string for the frontend
    string Status,
    string Title,
    DateTimeOffset CreatedAt,
    int WaitingMinutes,      // how long it's been sitting unassigned — for the "X min unassigned" UI
    string AssignmentReason  // e.g. "FactorBased: not implemented yet — queued for manual dispatch"
);

/// <summary>
/// Summary returned alongside the list so the frontend can render queue health
/// (badge counts, oldest-waiting indicator) without a second call.
/// </summary>
public sealed record GroupQueueSummaryDto(
    Guid GroupId,
    string GroupName,
    int TotalQueued,
    int OldestWaitingMinutes,
    IReadOnlyList<GroupQueueTicketDto> Tickets
);
