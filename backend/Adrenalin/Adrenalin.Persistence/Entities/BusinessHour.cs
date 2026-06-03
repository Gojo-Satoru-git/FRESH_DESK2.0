using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class BusinessHour
{
    public Guid Id { get; set; }

    public string GeoRegion { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string WorkingDays { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual GeoRegion GeoRegionNavigation { get; set; } = null!;
}
