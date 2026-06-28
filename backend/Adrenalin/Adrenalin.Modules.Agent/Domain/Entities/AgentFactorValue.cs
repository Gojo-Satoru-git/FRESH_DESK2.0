using System;

namespace Adrenalin.Modules.Agent.Domain.Entities;

public class AgentFactorValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public Guid FactorMasterId { get; set; }
    public Guid FactorValueId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual AgentEntity Agent { get; set; } = null!;
}