using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// M1-M4 customer tiers. priority_bump is used by SLA engine to elevate priority at creation.
/// </summary>
public partial class CustomerTier : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public int PriorityBump { get; set; }
}
