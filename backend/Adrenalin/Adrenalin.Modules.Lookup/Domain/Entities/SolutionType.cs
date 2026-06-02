using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class SolutionType : BaseEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
     public DateTimeOffset CreatedAt { get; private set; }

    private SolutionType() { }
}