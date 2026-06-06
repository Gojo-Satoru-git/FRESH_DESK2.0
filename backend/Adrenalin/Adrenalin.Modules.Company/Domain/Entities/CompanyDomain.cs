using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class CompanyDomain : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }

    public string Domain { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    public new string? CreatedBy { get; private set; }

    public Company Company { get; private set; } = null!;
}
