using System;
using System.Collections.Generic;
using Adrenalin.unify.API.Models.AuthModels;

namespace Adrenalin.unify.API.Models.Lookup;

/// <summary>
/// Named product release lines.
/// Code must be lowercase for scope resolver.
/// Soft-deleted versions remain on historical tickets
/// but are hidden from new-ticket UI.
/// </summary>
public partial class ProductVersion
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public bool IsLegacy { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}