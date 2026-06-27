using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Comments;

public sealed record CommentDto(
    Guid Id,
    Guid? AuthorId,
    Guid? ContactId,
    string Body,
    string Visibility,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<AttachmentDto> Attachments,
    string? AuthorName = null,
    string? ContactName = null
);