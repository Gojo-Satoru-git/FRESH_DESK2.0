using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class Company : SoftDeleteEntity
{
    public string Name { get; private set; } = null!;

    public string? CspId { get; private set; }

    public string GeoRegion { get; private set; } = null!;

    public string SupportTier { get; private set; } = null!;

    public string? Industry { get; private set; }

    public int HealthScore { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsPayrollCustomer { get; private set; }

    public string? LeaveCreditCycle { get; private set; }

    public DateOnly? MigrationDate { get; private set; }

    public Guid? CamUserId { get; private set; }

    public Guid? DeliveryManagerId { get; private set; }

    public bool DeliverySupportActive { get; private set; }

    public bool PaymentOnHold { get; private set; }

    public string? Notes { get; private set; }

    public DateTime? ModifiedAt { get; private set; }

    public string? ModifiedBy { get; private set; }

    public CompanyContactsLimit? CompanyContactsLimit { get; private set; }

    public ICollection<CompanyDomain> CompanyDomains { get; private set; } = new List<CompanyDomain>();

    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();
}
