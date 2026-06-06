using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class CompanyContactsLimit : BaseEntity
{
    public Guid CompanyId { get; private set; }

    public int MaxContacts { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Company Company { get; private set; } = null!;
}
