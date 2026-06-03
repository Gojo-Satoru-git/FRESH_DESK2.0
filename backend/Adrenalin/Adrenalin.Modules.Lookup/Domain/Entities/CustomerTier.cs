using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class CustomerTier : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public int PriorityBump { get; set; }
}
