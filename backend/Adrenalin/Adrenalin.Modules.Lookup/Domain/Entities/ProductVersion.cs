using System;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Named product release lines. code must be lowercase for scope resolver. Soft-deleted versions remain on historical tickets but are hidden from new-ticket UI.
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
}
