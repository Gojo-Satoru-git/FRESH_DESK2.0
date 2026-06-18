using System;

namespace Adrenalin.SharedKernel.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedOn { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
