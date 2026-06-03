using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class EscalationRule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int NoResponseMinutes { get; set; }

    public string NotifyRole { get; set; } = null!;

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
