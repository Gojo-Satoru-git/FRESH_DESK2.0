using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class CustomerTier : SoftDeleteEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int PriorityBump { get; private set; }

    private CustomerTier() { }
}