using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class PortalBanner
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? ActiveFrom { get; set; }

    public DateTime? ActiveTo { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
