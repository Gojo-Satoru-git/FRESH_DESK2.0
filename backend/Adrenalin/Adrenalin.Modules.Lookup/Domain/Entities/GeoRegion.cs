using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class GeoRegion : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;

    public string Timezone { get; private set; } = null!;

    public TimeOnly BusinessStart { get; private set; }

    public TimeOnly BusinessEnd { get; private set; }

    public string WorkingDays { get; private set; } = null!;
}
