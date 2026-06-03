using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Named status transition graphs. graph_id is resolved once at ticket creation via ticket_status_graph_scopes priority engine and stored on tickets.graph_id. Subsequent transitions are validated against this graph_id only.
/// </summary>
public partial class TicketStatusGraph
{
    public Guid Id { get; set; }

    public string GraphCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<StatusTransition> StatusTransitions { get; set; } = new List<StatusTransition>();

    public virtual ICollection<TicketStatusGraphScope> TicketStatusGraphScopes { get; set; } = new List<TicketStatusGraphScope>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User? UpdatedByNavigation { get; set; }
}
