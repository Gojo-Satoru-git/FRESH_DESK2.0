using System;
using System.Collections.Generic;
using Adrenalin.unify.API.Models.AuthModels;

namespace Adrenalin.unify.API.Models.Lookup;

/// <summary>
/// M1-M4 customer tiers. PriorityBump is used by SLA engine
/// to elevate priority at creation.
/// </summary>
public partial class CustomerTier
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public int PriorityBump { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Group> Groups { get; set; }
        = new List<Group>();
}