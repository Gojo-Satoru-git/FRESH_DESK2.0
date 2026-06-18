using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities.Email;

public sealed class EmailMessage : BaseEntity
{
    public Guid? TicketId { get; set; }
    public Guid? TicketCommentId { get; set; }
    
    public string Provider { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public string InternetMessageId { get; set; } = null!;
    public string? ThreadId { get; set; }
    public string? InReplyTo { get; set; }
    public string? References { get; set; }
    
    public string SenderEmail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string RecipientEmail { get; set; } = null!;
    public IReadOnlyList<string> CcEmails { get; set; } = new List<string>();
    
    public string Subject { get; set; } = null!;
    public string? BodyHtml { get; set; }
    public string? BodyText { get; set; }
    
    public DateTimeOffset ReceivedAt { get; set; }
    public string? LastOutboundMessageId { get; set; }
    
    public EmailProcessingState ProcessingState { get; set; } = EmailProcessingState.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // AI Ready Metadata
    public string? Language { get; set; }
    public string? SenderDomain { get; set; }
    public decimal? SpamScore { get; set; }
    public string? EmailClassification { get; set; }
    public string? DetectedIntent { get; set; }
    public string? Sentiment { get; set; }

    private readonly List<EmailAttachment> _attachments = new();
    public IReadOnlyCollection<EmailAttachment> Attachments => _attachments;

    public void AddAttachment(EmailAttachment attachment)
    {
        attachment.EmailMessageId = this.Id;
        _attachments.Add(attachment);
    }
}
