using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Workflow.Domain.Entities;

public sealed class StatusTransition : SoftDeleteEntity
{
    public Guid GraphId { get; private set; }

    public string? FromStatus { get; private set; }

    public string ToStatus { get; private set; } = null!;

    public Guid? RequiresRoleId { get; private set; }

    public string? RequiresField { get; private set; }

    public bool AutoTrigger { get; private set; }

    public int DisplayOrder { get; private set; }

    public TicketStatusGraph Graph { get; private set; } = null!;
}
