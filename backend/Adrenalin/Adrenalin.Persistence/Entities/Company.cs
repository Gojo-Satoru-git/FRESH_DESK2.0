using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Core customer account. health_score + customer_tiers.priority_bump elevates ticket priority. delivery_support_active routes tickets to delivery (hypercare). payment_on_hold triggers auto On Hold status for new tickets.
/// </summary>
public partial class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? CspId { get; set; }

    public string GeoRegion { get; set; } = null!;

    public string SupportTier { get; set; } = null!;

    public string? Industry { get; set; }

    public int HealthScore { get; set; }

    public bool IsActive { get; set; }

    public bool IsPayrollCustomer { get; set; }

    public string? LeaveCreditCycle { get; set; }

    public DateOnly? MigrationDate { get; set; }

    public Guid? CamUserId { get; set; }

    public Guid? DeliveryManagerId { get; set; }

    public bool DeliverySupportActive { get; set; }

    public bool PaymentOnHold { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual User? CamUser { get; set; }

    public virtual CompanyContactsLimit? CompanyContactsLimit { get; set; }

    public virtual ICollection<CompanyDomain> CompanyDomains { get; set; } = new List<CompanyDomain>();

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? DeliveryManager { get; set; }

    public virtual GeoRegion GeoRegionNavigation { get; set; } = null!;

    public virtual CustomerTier SupportTierNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User? UpdatedByNavigation { get; set; }
}
