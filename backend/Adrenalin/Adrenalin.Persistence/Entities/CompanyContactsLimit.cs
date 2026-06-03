using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class CompanyContactsLimit
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public int MaxContacts { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }
}
