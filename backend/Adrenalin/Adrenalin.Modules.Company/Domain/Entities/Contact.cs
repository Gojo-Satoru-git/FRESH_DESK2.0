using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class Contact : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string? Phone { get; private set; }

    public bool IsAuthorized { get; private set; }

    public bool AutoCreated { get; private set; }

    public DateTime? ModifiedAt { get; private set; }

    public string? ModifiedBy { get; private set; }

    public Company Company { get; private set; } = null!;
}
