using System;
using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Exceptions;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class CompanyContactsLimit : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public int MaxContacts { get; private set; }

    public Company Company { get; private set; } = null!;

    private static void Validate(int maxContacts)
    {
        if (maxContacts < 1)
        {
            throw new DomainException("Contact limit must be greater than zero.");
        }

        if (maxContacts > 100)
        {
            throw new DomainException("Contact limit cannot exceed 100.");
        }
    }
    public static CompanyContactsLimit Create(Guid companyId, int maxContacts)
    {
        Validate(maxContacts);

        return new CompanyContactsLimit
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            MaxContacts = maxContacts
        };
    }

    public void ChangeLimit(int maxContacts, Guid modifiedById)
    {
        Validate(maxContacts);

        MaxContacts = maxContacts;

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = modifiedById;
    }
}
