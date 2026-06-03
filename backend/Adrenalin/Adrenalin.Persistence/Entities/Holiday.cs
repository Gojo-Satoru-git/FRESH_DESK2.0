using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class Holiday
{
    public Guid Id { get; set; }

    public string GeoRegion { get; set; } = null!;

    public DateOnly HolidayDate { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual GeoRegion GeoRegionNavigation { get; set; } = null!;
}
