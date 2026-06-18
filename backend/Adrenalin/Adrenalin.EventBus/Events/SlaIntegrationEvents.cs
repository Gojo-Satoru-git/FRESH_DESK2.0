namespace Adrenalin.EventBus.Events;

public sealed record SlaBreachedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    string BreachType,
    string EscalationRuleName,
    string TargetRole,
    List<Guid> TargetUserIds) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
