using System;
using System.Collections.Generic;

namespace Adrenalin.EventBus.Events;

public sealed record TicketCreatedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    string Title,
    Guid? ReporterId,
    Guid? AssigneeId,
    string Category,
    string Priority,
    string? Department,
    string? Region
);

public sealed record TicketAssignedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    Guid AssigneeId,
    Guid AssignedBy,
    string? Notes
);

public sealed record TicketResolvedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    Guid ResolvedBy,
    string? ResolutionSummary
);

public sealed record TicketClosedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    Guid ClosedBy,
    string? Notes
);

public sealed record TicketCommentAddedIntegrationEvent(
    Guid TicketId,
    Guid CommentId,
    string Body,
    List<string> MentionedUsers,
    Guid? AuthorId,
    Guid? ContactId,
    string Visibility
);

public sealed record TicketReopenedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    Guid ReopenedBy,
    string? Notes
);

public sealed record TicketMergedIntegrationEvent(
    Guid TicketId,
    string TicketNumber,
    string MasterTicketNumber,
    Guid MergedBy
);
