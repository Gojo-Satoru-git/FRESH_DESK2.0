using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Top-level product functional areas. is_mandatory_for_closure enforces module selection before ticket resolution. department drives default graph scope lookup.
/// </summary>
public partial class Module
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public string? Department { get; set; }

    public bool IsMandatoryForClosure { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ICollection<SubModule> SubModules { get; set; } = new List<SubModule>();
}
