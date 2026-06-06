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
}
