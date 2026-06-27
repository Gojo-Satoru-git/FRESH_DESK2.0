using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
public sealed record AddCommentRequest(
    Guid? AuthorId,
    Guid? ContactId,
    string Body,
    bool IsPrivate
);
