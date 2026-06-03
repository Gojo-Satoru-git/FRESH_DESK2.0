using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Maps every agent-facing ticket status to the simplified customer-facing label. Customer portal API reads this table — customers never see statuses like pending_internal or compliance_review. Only Open, Reopen, Closed are exposed.
/// </summary>
public partial class CustomerStatusMap
{
    public Guid Id { get; set; }

    public string AgentStatus { get; set; } = null!;

    public string CustomerLabel { get; set; } = null!;

    public string? CustomerDescription { get; set; }

    public bool IsActive { get; set; }
}
