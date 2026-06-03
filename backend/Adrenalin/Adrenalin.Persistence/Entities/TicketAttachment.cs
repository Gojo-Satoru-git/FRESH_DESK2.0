using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class TicketAttachment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? CommentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public long? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TicketComment? Comment { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
