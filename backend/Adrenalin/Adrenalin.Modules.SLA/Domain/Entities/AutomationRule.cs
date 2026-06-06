using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class AutomationRule : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = null!;

    public string Conditions { get; private set; } = null!;

    public string Actions { get; private set; } = null!;

    public int ExecutionOrder { get; private set; }

    public ICollection<AutomationExecutionLog> AutomationExecutionLogs { get; private set; } = new List<AutomationExecutionLog>();
}
