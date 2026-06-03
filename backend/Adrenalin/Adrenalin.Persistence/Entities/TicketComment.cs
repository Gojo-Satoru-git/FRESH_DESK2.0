using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class TicketComment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? AuthorId { get; set; }

    public Guid? ContactId { get; set; }

    public string Body { get; set; } = null!;

    public bool IsPrivate { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? Author { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();
}
