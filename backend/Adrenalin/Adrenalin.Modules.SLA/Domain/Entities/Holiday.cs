using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class Holiday : SoftDeleteEntity
{
    public string GeoRegion { get; private set; } = null!;

    public DateOnly HolidayDate { get; private set; }

    public string Name { get; private set; } = null!;
}
