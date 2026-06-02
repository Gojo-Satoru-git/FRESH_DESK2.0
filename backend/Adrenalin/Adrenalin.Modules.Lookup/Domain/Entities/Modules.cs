using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class Module : SoftDeleteEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Department { get; private set; }

    public bool IsMandatoryForClosure { get; private set; }

    public ICollection<SubModule> SubModules { get; private set; }
        = new List<SubModule>();

    private Module() { }
}