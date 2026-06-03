using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Authorized contacts per company (5-20 per account). is_authorized=false blocks ticket creation. auto_created=true when system creates contact from unknown inbound email domain match.
/// </summary>
public partial class Contact
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsAuthorized { get; set; }

    public bool AutoCreated { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<CsatSurvey> CsatSurveys { get; set; } = new List<CsatSurvey>();

    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual User? User { get; set; }
}
