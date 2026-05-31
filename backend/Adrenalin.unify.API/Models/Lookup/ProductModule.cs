using System;
using System.Collections.Generic;
using Adrenalin.unify.API.Models.AuthModels;

namespace Adrenalin.unify.API.Models.Lookup;

/// <summary>
/// Top-level product functional areas.
/// IsMandatoryForClosure enforces module selection before ticket resolution.
/// Department drives default graph scope lookup.
/// </summary>
public partial class ProductModule
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

    // Audit Relationships
    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    // Child Relationship
    public virtual ICollection<SubModule> SubModules { get; set; }
        = new List<SubModule>();
}