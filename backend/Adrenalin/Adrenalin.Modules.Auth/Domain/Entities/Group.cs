using System;
using System.Collections.Generic;


namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// 13 geo/tier groups. RegionCode and TierCode determine SLA policy and
/// routing scope. Admin can create additional groups as org scales.
/// </summary>
public partial class Group
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? RegionCode { get; set; }

    public string? TierCode { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? RowVersion { get; set; }

    // Audit Relationships
    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    // Lookup Relationships
    public virtual GeoRegion? RegionCodeNavigation { get; set; }

    public virtual CustomerTier? TierCodeNavigation { get; set; }

    // User Group Relationships
    public virtual ICollection<UserGroup> UserGroups { get; set; }
        = new List<UserGroup>();
}