using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketStatusHistory : BaseEntity
{
    public Ticket Ticket { get; private set; } = null!;
    public Guid TicketId { get; private set; }
    public TicketStatus? FromStatus { get; private set; }
    public TicketStatus ToStatus { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Reason { get; private set; }
    private TicketStatusHistory() {}

    private TicketStatusHistory(Guid ticketId, TicketStatus? fromStatus, TicketStatus toStatus, Guid? changedBy, string? reason)
    {
        TicketId = ticketId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedBy = changedBy;
        Reason = reason;
        ChangedAt = DateTimeOffset.UtcNow;
    }
    public static TicketStatusHistory Create(Guid ticketId, TicketStatus? fromStatus, TicketStatus toStatus, Guid? changedBy, string? reason = null)
    {
        if (ticketId == Guid.Empty)
            throw new TicketDomainException("TicketId cannot be empty.");

        if (fromStatus.HasValue && !Enum.IsDefined(typeof(TicketStatus), fromStatus.Value))
            throw new TicketDomainException("Invalid 'From' status provided.");

        if (!Enum.IsDefined(typeof(TicketStatus), toStatus))
            throw new TicketDomainException("Invalid 'To' status provided.");

        if (fromStatus.HasValue && fromStatus.Value == toStatus)
            throw new TicketDomainException("FromStatus and ToStatus cannot be the same.");

        if (changedBy.HasValue && changedBy.Value == Guid.Empty)
            throw new TicketDomainException("ChangedBy user ID cannot be empty.");

        if (!string.IsNullOrWhiteSpace(reason) && reason.Length > 2000)
            throw new TicketDomainException("Reason cannot exceed 2000 characters.");

        return new TicketStatusHistory(ticketId, fromStatus, toStatus, changedBy, reason);
    }
}