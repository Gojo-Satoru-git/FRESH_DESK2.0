using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Workflow.Domain.Entities;

public sealed class TicketStatusGraph : ActiveSoftDeleteEntity
{
    public string GraphCode { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public ICollection<StatusTransition> StatusTransitions { get; private set; } = new List<StatusTransition>();

    public ICollection<TicketStatusGraphScope> TicketStatusGraphScopes { get; private set; } = new List<TicketStatusGraphScope>();
}
