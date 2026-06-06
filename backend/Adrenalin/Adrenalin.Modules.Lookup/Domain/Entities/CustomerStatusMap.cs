using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

public sealed class CustomerStatusMap
{
    public Guid Id { get; private set; }

    public string AgentStatus { get; private set; } = null!;

    public string CustomerLabel { get; private set; } = null!;

    public string? CustomerDescription { get; private set; }

    public bool IsActive { get; private set; }
}
