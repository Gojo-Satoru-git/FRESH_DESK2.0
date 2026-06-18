using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities.Email;

public sealed class EmailAttachment : BaseEntity
{
    public Guid EmailMessageId { get; set; }
    public Guid? TicketAttachmentId { get; set; }
    
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public string? Hash { get; set; }
    public string? StoragePath { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public EmailMessage EmailMessage { get; set; } = null!;
}
