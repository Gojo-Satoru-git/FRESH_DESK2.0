using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class CustomerTier : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;

    public string? Description { get; private set; }

    public int PriorityBump { get; private set; }
}
