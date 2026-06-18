namespace Adrenalin.EventBus.Events;

public sealed record GroupLeadPromotedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid GroupId,
    Guid PromotedUserId,
    string PromotedUserRole) : IIntegrationEvent;
