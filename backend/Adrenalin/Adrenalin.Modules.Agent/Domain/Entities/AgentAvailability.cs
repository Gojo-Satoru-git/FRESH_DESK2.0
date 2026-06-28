using System;

namespace Adrenalin.Modules.Agent.Domain.Entities;

public class AgentAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public Guid? ShiftFactorValueId { get; set; }
    public string? Timezone { get; set; }
    public TimeOnly? ShiftStart { get; set; }
    public TimeOnly? ShiftEnd { get; set; }
    public string? WorkingDays { get; set; } // Stored as string to map to JSONB format JSON
    public bool OnCallOverride { get; set; } = false;
    public DateTimeOffset? OnCallExpiry { get; set; }
    public bool OutOfOffice { get; set; } = false;
    public DateTimeOffset? OutOfOfficeStart { get; set; }
    public DateTimeOffset? OutOfOfficeEnd { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual AgentEntity Agent { get; set; } = null!;
}