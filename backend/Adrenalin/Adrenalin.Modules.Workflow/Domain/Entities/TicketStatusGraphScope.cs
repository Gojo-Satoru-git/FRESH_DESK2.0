using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Workflow.Domain.Entities;

public sealed class TicketStatusGraphScope : ActiveSoftDeleteEntity
{
    public Guid GraphId { get; private set; }

    public string? Department { get; private set; }

    public Guid? VersionId { get; private set; }

    public Guid? ModuleId { get; private set; }

    public Guid? SubModuleId { get; private set; }

    public int Priority { get; private set; }

    public TicketStatusGraph Graph { get; private set; } = null!;
}
