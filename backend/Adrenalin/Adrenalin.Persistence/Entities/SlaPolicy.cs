using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// One row per (geo_region × tier × priority). First response: Urgent=120m, High=240m, Med=360m, Low=480m. Resolution: Low=2880m (48 biz hrs). SLA minutes are business-hours only, not wall-clock.
/// </summary>
public partial class SlaPolicy
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string GeoRegion { get; set; } = null!;

    public string TierCode { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public int FirstResponseMinutes { get; set; }

    public int ResolutionMinutes { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual GeoRegion GeoRegionNavigation { get; set; } = null!;

    public virtual ICollection<SlaTicket> SlaTickets { get; set; } = new List<SlaTicket>();

    public virtual CustomerTier TierCodeNavigation { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
