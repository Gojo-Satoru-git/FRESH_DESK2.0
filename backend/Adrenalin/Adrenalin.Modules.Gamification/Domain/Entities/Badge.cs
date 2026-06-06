using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Gamification.Domain.Entities;

public sealed class Badge : BaseEntity
{
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? IconUrl { get; private set; }

    public int PointsValue { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<AgentBadge> AgentBadges { get; private set; } = new List<AgentBadge>();

    public ICollection<Challenge> Challenges { get; private set; } = new List<Challenge>();
}
