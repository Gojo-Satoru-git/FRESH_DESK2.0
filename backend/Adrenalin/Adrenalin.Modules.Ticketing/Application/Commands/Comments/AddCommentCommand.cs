using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record AddCommentCommand(
    Guid TicketId,
    Guid? AuthorId,
    Guid? ContactId,
    string Body,
    bool IsPrivate
) : IRequest<Guid>;
