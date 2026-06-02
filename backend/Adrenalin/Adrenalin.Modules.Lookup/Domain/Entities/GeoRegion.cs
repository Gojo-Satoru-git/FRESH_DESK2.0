using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class GeoRegion :SoftDeleteEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string Timezone { get; private set; } = string.Empty;

    public TimeOnly BusinessStart { get; private set; }

    public TimeOnly BusinessEnd { get; private set; }

    public string WorkingDays { get; private set; } = string.Empty;

    private GeoRegion() { }
}