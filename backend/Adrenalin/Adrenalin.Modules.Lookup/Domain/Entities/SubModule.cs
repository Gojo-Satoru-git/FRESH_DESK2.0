using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class SubModule : ActiveSoftDeleteEntity
{
    public Guid ModuleId { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public bool RequiresDualConfirm { get; set; }

    public bool RequiresComplianceReview { get; set; }

    public virtual Module Module { get; set; } = null!;
}
