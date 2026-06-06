using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class SlaPolicy : ActiveSoftDeleteEntity
{
    public string GeoRegion { get; private set; } = null!;

    public string TierCode { get; private set; } = null!;

    public string Priority { get; private set; } = null!;

    public int FirstResponseMinutes { get; private set; }

    public int ResolutionMinutes { get; private set; }

    public string Name { get; private set; } = null!;

    public ICollection<SlaTicket> SlaTickets { get; private set; } = new List<SlaTicket>();
}
