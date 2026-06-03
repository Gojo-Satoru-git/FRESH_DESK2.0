using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Email domains owned by each company. UNIQUE on lower(domain) enables O(1) auto-routing of incoming emails to the correct company account.
/// </summary>
public partial class CompanyDomain
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Domain { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Company Company { get; set; } = null!;
}
