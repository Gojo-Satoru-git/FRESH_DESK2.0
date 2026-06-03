using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class AutomationExecutionLog
{
    public Guid Id { get; set; }

    public Guid RuleId { get; set; }

    public Guid TicketId { get; set; }

    public bool Succeeded { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime ExecutedAt { get; set; }

    public virtual AutomationRule Rule { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
