using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class Module : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public string? Department { get; set; }

    public bool IsMandatoryForClosure { get; set; }

    public virtual ICollection<SubModule> SubModules { get; set; } = new List<SubModule>();
}
