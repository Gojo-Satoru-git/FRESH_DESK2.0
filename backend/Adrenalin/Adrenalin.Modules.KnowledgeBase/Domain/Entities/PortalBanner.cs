using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.KnowledgeBase.Domain.Entities;

public sealed class PortalBanner
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Message { get; private set; } = null!;

    public DateTime? ActiveFrom { get; private set; }

    public DateTime? ActiveTo { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }
}
