using System;

namespace Adrenalin.Modules.Agent.Domain.Entities;

public class AgentAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public string Action { get; set; } = null!;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public virtual AgentEntity Agent { get; set; } = null!;
}