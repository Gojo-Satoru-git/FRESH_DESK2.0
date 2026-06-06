using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class EscalationRule : BaseEntity
{
    public string Name { get; private set; } = null!;

    public int NoResponseMinutes { get; private set; }

    public string NotifyRole { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
