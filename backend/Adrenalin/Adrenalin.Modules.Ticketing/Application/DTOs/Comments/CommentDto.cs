namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record CommentDto(
    Guid Id,
    Guid? AuthorId,
    Guid? ContactId,
    string Body,
    string Visibility,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<AttachmentDto> Attachments
);