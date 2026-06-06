using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class PointRule : BaseEntity
{
    public string Code { get; private set; } = null!;

    public string? Description { get; private set; }

    public int Points { get; private set; }

    public int? MaxPerDay { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ICollection<AgentPoint> AgentPoints { get; private set; } = new List<AgentPoint>();
}
