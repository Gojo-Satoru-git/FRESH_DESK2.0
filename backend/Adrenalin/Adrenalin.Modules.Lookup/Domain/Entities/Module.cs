using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class Module : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Department { get; private set; }

    public bool IsMandatoryForClosure { get; private set; }

    public ICollection<SubModule> SubModules { get; private set; } = new List<SubModule>();

    public static Module Create(string code, string label, string? department = null, bool isMandatoryForClosure = false)
    {
        return new Module
        {
            Id = Guid.NewGuid(),
            Code = code,
            Label = label,
            Department = department,
            IsMandatoryForClosure = isMandatoryForClosure,
            IsActive = true
        };
    }
}
