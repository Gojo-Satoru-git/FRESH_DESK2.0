using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

/// <summary>
/// Tracks every group assignment or reassignment for a ticket.
/// Used for audit trail and routing analytics.
/// </summary>
public sealed class GroupAssignmentHistory : AuditableEntity
{
    public Guid TicketId { get; private set; }
    public Guid? PreviousGroupId { get; private set; }
    public Guid? NewGroupId { get; private set; }
    public Guid AssignedBy { get; private set; }
    public string? Reason { get; private set; }

    /// <summary>
    /// The name/identifier of the routing rule that produced this assignment.
    /// Examples: "CompanyExplicit:Rule#3", "RegionMatch:APAC", "Fallback", "ManualAssignment".
    /// </summary>
    public string? RoutingRuleMatched { get; private set; }

    private GroupAssignmentHistory() { }

    public static GroupAssignmentHistory Create(
        Guid ticketId,
        Guid? previousGroupId,
        Guid? newGroupId,
        Guid assignedBy,
        string? reason,
        string? routingRuleMatched)
    {
        if (ticketId == Guid.Empty) throw new ArgumentException("TicketId is required.");

        return new GroupAssignmentHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            PreviousGroupId = previousGroupId,
            NewGroupId = newGroupId,
            AssignedBy = assignedBy,
            Reason = reason,
            RoutingRuleMatched = routingRuleMatched,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = assignedBy
        };
    }
}
