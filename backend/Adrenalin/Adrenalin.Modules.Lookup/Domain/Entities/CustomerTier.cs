using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class CustomerTier : ActiveSoftDeleteEntity
{
    public string Code { get; private set; } = null!;

    public string Label { get; private set; } = null!;

    public string? Description { get; private set; }

    public int PriorityBump { get; private set; }

    public static CustomerTier Create(string code, string label, string? description = null, int priorityBump = 0)
    {
        return new CustomerTier
        {
            Id = Guid.NewGuid(),
            Code = code,
            Label = label,
            Description = description,
            PriorityBump = priorityBump,
            IsActive = true
        };
    }
}
