using System;

namespace Adrenalin.Modules.Agent.Domain.Entities;

public class AgentStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = null!;
    public string? Reason { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public virtual AgentEntity Agent { get; set; } = null!;
}