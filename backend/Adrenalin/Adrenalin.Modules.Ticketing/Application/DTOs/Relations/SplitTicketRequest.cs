using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Relations;

public sealed record SplitTicketRequest(
    string NewSubject,
    string NewDescription,
    Guid CreatedByUserId,
    List<Guid>? CommentIdsToMove,
    List<Guid>? AttachmentIdsToMove
);
