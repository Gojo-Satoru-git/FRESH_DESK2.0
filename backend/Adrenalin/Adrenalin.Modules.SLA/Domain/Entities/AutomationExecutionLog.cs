using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class AutomationExecutionLog : BaseEntity
{
    public Guid RuleId { get; private set; }
    public Guid TicketId { get; private set; }
    public bool Succeeded { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime ExecutedAt { get; private set; }

    public AutomationRule Rule { get; private set; } = null!;

    private AutomationExecutionLog() { }  // ← add this for EF Core

    // ← add this factory method
    public static AutomationExecutionLog Create(
        Guid ruleId,
        Guid ticketId,
        bool succeeded = true,
        string? errorMessage = null)
    {
        return new AutomationExecutionLog
        {
            RuleId = ruleId,
            TicketId = ticketId,
            Succeeded = succeeded,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTime.UtcNow
        };
    }
}