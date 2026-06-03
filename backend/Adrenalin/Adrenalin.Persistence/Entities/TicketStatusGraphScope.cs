using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Scope-to-graph mapping. NULL dimensions act as wildcards. Resolver evaluates rows by priority DESC and returns the first match. GRAPH-001 (priority=0) is the default fallback. Uses FK references to product_versions, modules, sub_modules — replaces old varchar fields.
/// </summary>
public partial class TicketStatusGraphScope
{
    public Guid Id { get; set; }

    public Guid GraphId { get; set; }

    public string? Department { get; set; }

    public Guid? VersionId { get; set; }

    public Guid? ModuleId { get; set; }

    public Guid? SubModuleId { get; set; }

    public int Priority { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual TicketStatusGraph Graph { get; set; } = null!;

    public virtual Module? Module { get; set; }

    public virtual SubModule? SubModule { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ProductVersion? Version { get; set; }
}
