using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class GeoRegion : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string Timezone { get; set; } = null!;

    public TimeOnly BusinessStart { get; set; }

    public TimeOnly BusinessEnd { get; set; }

    public string WorkingDays { get; set; } = null!;
}
