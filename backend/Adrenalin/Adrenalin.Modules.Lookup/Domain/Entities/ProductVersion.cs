using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Named product release lines. code must be lowercase for scope resolver. Soft-deleted versions remain on historical tickets but are hidden from new-ticket UI.
/// </summary>
public partial class ProductVersion : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public bool IsLegacy { get; set; }
}
