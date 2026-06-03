using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public partial class ProductVersion : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public bool IsLegacy { get; set; }
}
