using System;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Region-specific business hours and timezone. SLA clock runs only within these windows.
/// </summary>
public partial class GeoRegion
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string Timezone { get; set; } = null!;

    public TimeOnly BusinessStart { get; set; }

    public TimeOnly BusinessEnd { get; set; }

    public string WorkingDays { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
