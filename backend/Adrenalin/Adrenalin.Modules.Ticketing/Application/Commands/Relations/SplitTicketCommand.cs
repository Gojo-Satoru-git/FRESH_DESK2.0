using Adrenalin.SharedKernel.Mediator;
using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Relations;

public sealed record SplitTicketCommand(
    Guid OriginalTicketId,
    string NewSubject,
    string NewDescription,
    Guid CreatedByUserId,
    List<Guid>? CommentIdsToMove = null,
    List<Guid>? AttachmentIdsToMove = null
) : IRequest<Guid>;
