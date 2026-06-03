using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class SolutionType : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;
}
