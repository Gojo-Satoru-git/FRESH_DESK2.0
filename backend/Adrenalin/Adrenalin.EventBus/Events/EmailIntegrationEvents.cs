using System;
using System.Collections.Generic;

namespace Adrenalin.EventBus.Events;

public sealed record EmailAttachmentDto(
    string FileName,
    string ContentType,
    long Size,
    string? ContentId,
    byte[] ContentBytes // Assuming small payloads passed through RabbitMQ, for very large it might be a stream/storage link
);

public sealed record EmailReceivedIntegrationEvent(
    Guid EventId,
    string Provider,
    string MessageId,
    string InternetMessageId,
    string? ThreadId,
    string? InReplyTo,
    string? References,
    string Subject,
    string? BodyText,
    string? BodyHtml,
    string FromEmail,
    string FromName,
    string ToEmail,
    IReadOnlyList<string> CcEmails,
    DateTimeOffset ReceivedAt,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyList<EmailAttachmentDto> Attachments
) : IIntegrationEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
