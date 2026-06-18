using System;

namespace Adrenalin.SharedKernel.Entities;

public class IntegrationEventLog
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTimeOffset ProcessedAt { get; set; }
}
