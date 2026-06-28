using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Agent.Domain.Entities;

public class AgentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? ProfilePhoto { get; set; }
    public string? PreferredName { get; set; }
    public string? EmployeeId { get; set; }
    public string? DisplayName { get; set; }
    public Guid? ManagerAgentId { get; set; }
    public int MaxConcurrentTickets { get; set; } = 10;
    public int CurrentWorkload { get; set; } = 0;
    public decimal UtilizationPercentage { get; set; } = 0.00m;
    public string Status { get; set; } = "offline";
    public DateTimeOffset? StatusSince { get; set; }
    public string? DeactivationReason { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual AgentEntity? Manager { get; set; }
    public virtual ICollection<AgentEntity> DirectReports { get; set; } = new List<AgentEntity>();
    public virtual AgentAvailability? Availability { get; set; }
    public virtual ICollection<AgentFactorValue> FactorValues { get; set; } = new List<AgentFactorValue>();
    public virtual ICollection<AgentStatusHistory> StatusHistories { get; set; } = new List<AgentStatusHistory>();
    public virtual ICollection<AgentAudit> AuditLogs { get; set; } = new List<AgentAudit>();
}