using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class ProductVersion : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;

    public DateOnly? ReleaseDate { get; private set; }

    public bool IsLegacy { get; private set; }
}
