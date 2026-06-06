using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class BusinessHour : SoftDeleteEntity
{
    public string GeoRegion { get; private set; } = null!;

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public string WorkingDays { get; private set; } = null!;
}
