using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class ProductVersion :SoftDeleteEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public DateOnly? ReleaseDate { get; private set; }

    public bool IsLegacy { get; private set; }

    private ProductVersion() { }
}