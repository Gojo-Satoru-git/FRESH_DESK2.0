using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;
public sealed record AddCommentRequest(
    Guid? AuthorId,
    Guid? ContactId,
    string Body,
    CommentVisibility Visibility
);