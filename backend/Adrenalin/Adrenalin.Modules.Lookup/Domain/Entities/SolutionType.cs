using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class SolutionType : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;
}
