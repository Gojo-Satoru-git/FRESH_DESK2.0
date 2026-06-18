using System;
using System.Collections.Generic;

namespace Adrenalin.EventBus.Events;

public sealed record TicketCreatedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    string Title,
    Guid? ReporterId,
    Guid? AssigneeId,
    string Category,
    string Priority,
    string? Department,
    string? Region
) : IIntegrationEvent;

public sealed record TicketAssignedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid AssigneeId,
    Guid AssignedBy,
    string? Notes
) : IIntegrationEvent;

public sealed record TicketGroupAssignedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid GroupId,
    Guid AssignedBy,
    string? Notes
) : IIntegrationEvent;

public sealed record TicketResolvedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid ResolvedBy,
    string? ResolutionSummary
) : IIntegrationEvent;

public sealed record TicketClosedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid ClosedBy,
    string? Notes
) : IIntegrationEvent;

public sealed record TicketCommentAddedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    Guid CommentId,
    string Body,
    List<string> MentionedUsers,
    Guid? AuthorId,
    Guid? ContactId,
    string Visibility
) : IIntegrationEvent;

public sealed record TicketReopenedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid ReopenedBy,
    string? Notes
) : IIntegrationEvent;

public sealed record TicketMergedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    string MasterTicketNumber,
    Guid MergedBy
) : IIntegrationEvent;

public sealed record TicketStatusChangedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    string FromStatus,
    string ToStatus,
    Guid ChangedBy,
    string? Reason
) : IIntegrationEvent;

public sealed record CustomerRepliedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid ContactId,
    string Body
) : IIntegrationEvent;

public sealed record WatcherAddedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid TicketId,
    string TicketNumber,
    Guid WatcherContactId,
    Guid AddedBy
) : IIntegrationEvent;
