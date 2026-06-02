using System;
using System.Collections.Generic;
using Adrenalin.Modules.Auth.Domain.Entities;


namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Fine-grained ticket classification.
/// RequiresDualConfirm triggers GRAPH-006.
/// RequiresComplianceReview triggers GRAPH-005.
/// Sub-module wins over module in scope resolution.
/// </summary>
public partial class SubModule
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public bool RequiresDualConfirm { get; set; }

    public bool RequiresComplianceReview { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ProductModule ProductModule { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}