using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketActivity : BaseEntity
{
    public Guid TicketId { get; private set; }
    public string ActivityType { get; private set; } = null!;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public Guid? PerformedBy { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }
    public Ticket Ticket { get; private set; } = null!;

    private TicketActivity() { }

    public static TicketActivity Create(Guid ticketId, string activityType, string? oldValue, string? newValue, Guid? performedBy)
    {
        return new TicketActivity
        {
            TicketId = ticketId,
            ActivityType = activityType,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedBy = performedBy,
            PerformedAt = DateTimeOffset.UtcNow
        };
    }
}
