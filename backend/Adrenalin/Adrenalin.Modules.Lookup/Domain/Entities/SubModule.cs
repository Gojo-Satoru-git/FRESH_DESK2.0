using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class SubModule : SoftDeleteEntity
{
    public Guid ModuleId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool RequiresDualConfirm { get; private set; }

    public bool RequiresComplianceReview { get; private set; }

    public Module Module { get; private set; } = null!;

    private SubModule() { }
}