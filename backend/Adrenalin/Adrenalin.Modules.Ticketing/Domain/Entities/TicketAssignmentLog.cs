using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketAssignmentLog : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid? FromAgentId { get; private set; }
    public Guid ToAgentId { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public string? Notes { get; private set; }
    public Ticket Ticket { get; private set; } = null!;

    private TicketAssignmentLog() { }
    private TicketAssignmentLog(Guid ticketId, Guid? fromAgentId, Guid toAgentId, Guid? changedBy, string? notes)
    {
        TicketId = ticketId;
        FromAgentId = fromAgentId;
        ToAgentId = toAgentId;
        ChangedBy = changedBy;
        Notes = notes;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public static TicketAssignmentLog Create(Guid ticketId, Guid? fromAgentId, Guid toAgentId, Guid? changedBy, string? notes = null)
    {
        if (ticketId == Guid.Empty)
            throw new TicketDomainException("TicketId cannot be empty.");

        if (toAgentId == Guid.Empty)
            throw new TicketDomainException("ToAgentId cannot be empty.");

        if (changedBy.HasValue && changedBy.Value == Guid.Empty)
            throw new TicketDomainException("ChangedBy cannot be empty.");

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 2000)
            throw new TicketDomainException("Notes cannot exceed 2000 characters.");

        var ticketAssignmentLog = new TicketAssignmentLog(
            ticketId,
            fromAgentId,
            toAgentId,
            changedBy,
            notes
        );

        return ticketAssignmentLog;
    }
}
