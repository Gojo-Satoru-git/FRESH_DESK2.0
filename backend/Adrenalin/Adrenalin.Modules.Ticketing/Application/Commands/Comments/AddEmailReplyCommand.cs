using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Comments;

public sealed record AddEmailReplyCommand(
    Guid TicketId,
    Guid EmailMessageId,
    Guid? ContactId,
    Guid? AuthorId,
    string PlainTextBody,
    string? HtmlBody,
    IReadOnlyList<Guid>? WatcherUserIds,
    bool IsPrivate,
    IReadOnlyList<Adrenalin.EventBus.Events.EmailAttachmentDto>? Attachments = null
) : IRequest<Result<Guid>>;
