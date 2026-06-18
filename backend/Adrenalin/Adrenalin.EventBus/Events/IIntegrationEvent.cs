using System;

namespace Adrenalin.EventBus.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
