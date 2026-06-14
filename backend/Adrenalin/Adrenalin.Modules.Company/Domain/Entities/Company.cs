using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Exceptions;
namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class Company : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = null!;

    public string? CspId { get; private set; }

    public string GeoRegion { get; private set; } = null!;

    public string SupportTier { get; private set; } = null!;

    public string? Industry { get; private set; }

    public int HealthScore { get; private set; }

    public bool IsPayrollCustomer { get; private set; }

    public string? LeaveCreditCycle { get; private set; }

    public DateOnly? MigrationDate { get; private set; }

    public Guid? CamUserId { get; private set; }

    public Guid? DeliveryManagerId { get; private set; }

    public bool DeliverySupportActive { get; private set; }

    public bool PaymentOnHold { get; private set; }

    public string? Notes { get; private set; }

    public CompanyContactsLimit? CompanyContactsLimit { get; private set; }

    private readonly List<Contact> _contacts = [];
    private readonly List<CompanyDomain> _domains = [];

    public IReadOnlyCollection<Contact> Contacts => _contacts;
    public IReadOnlyCollection<CompanyDomain> CompanyDomains => _domains;

    private Company() { }

    public static Company Create(string name, string geoRegion, string supportTier, Guid? createdById = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(geoRegion);
        ArgumentException.ThrowIfNullOrWhiteSpace(supportTier);

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            GeoRegion = geoRegion.Trim(),
            SupportTier = supportTier.Trim(),
            IsActive = true,
            HealthScore = 100
        };

        // If you want to set the initial creation audit manually:
        // company.CreatedAt = DateTimeOffset.UtcNow;
        // company.CreatedBy = createdById;

        // company.RaiseDomainEvent(new CompanyDomainEvent(company.Id));

        return company;
    }

    public void UpdateDetails(string name, string? industry, string supportTier, Guid modifiedById)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Industry = industry;
        SupportTier = supportTier;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyUpdatedDomainEvent(Id));
    }

    public void Activate(Guid modifiedById)
    {
        if (IsActive) return;

        IsActive = true;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyActivatedDomainEvent(Id));
    }

    public void Deactivate(Guid modifiedById)
    {
        if (!IsActive) return;

        IsActive = false;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyDeactivatedDomainEvent(Id));
    }

    public void AssignCam(Guid userId, Guid modifiedById)
    {
        if (CamUserId == userId) return;

        CamUserId = userId;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyCamAssignedDomainEvent(Id, userId));
    }

    public void AssignDeliveryManager(Guid userId, Guid modifiedById)
    {
        if (DeliveryManagerId == userId) return;

        DeliveryManagerId = userId;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyDeliveryManagerAssignedDomainEvent(Id, userId));
    }

    public void UpdateHealthScore(int score, Guid modifiedById)
    {
        if (score < 0 || score > 100)
        {
             throw new DomainException("Health score must be between 0 and 100.");
        }

        if (HealthScore == score) return;

        HealthScore = score;

        SetAudit(modifiedById);

        // RaiseDomainEvent(new CompanyHealthScoreChangedDomainEvent(Id, score));
    }

    public void SetPaymentHold(bool value, Guid modifiedById)
    {
        if (PaymentOnHold == value) return;

        PaymentOnHold = value;

        SetAudit(modifiedById);
    }

    public void SetDeliverySupport(bool enabled, Guid modifiedById)
    {
        if (DeliverySupportActive == enabled) return;

        DeliverySupportActive = enabled;

        SetAudit(modifiedById);
    }

    public void AddDomain(string domain, bool isPrimary)
    {
        var companyDomain = CompanyDomain.Create(Id, domain, isPrimary);

        if (isPrimary)
        {
            foreach (var existing in _domains)
            {
                existing.UnsetPrimary();
            }
        }

        _domains.Add(companyDomain);
    }

    public void SetPrimaryDomain(Guid domainId)
    {
        foreach (var domain in _domains)
        {
            domain.UnsetPrimary();
        }

        var primary = _domains.Single(x => x.Id == domainId);
        primary.SetPrimary();
    }

    public Contact AddContact(string name, string email, string? phone, bool autoCreated = false)
    {
        var maxContacts = CompanyContactsLimit?.MaxContacts ?? 20;

        if (_contacts.Count >= maxContacts)
        {
             throw new DomainException($"Maximum contact limit of {maxContacts} reached.");
        }

        if (_contacts.Any(x => x.Email == email.ToLowerInvariant()))
        {
             throw new DomainException("Contact already exists.");
        }

        var contact = Contact.Create(Id, email, name, autoCreated);
        _contacts.Add(contact);

        return contact;
    }

    private void SetAudit(Guid modifiedById)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = modifiedById;
    }
}