using System;
using System.Collections.Generic;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Domain.Events;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    string TicketNumber,
    string Title,
    Guid? ReporterId,
    Guid? AssigneeId,
    TicketCategory Category,
    TicketPriority Priority,
    string? Department,
    string? Region
) : INotification;

public sealed record TicketAssignedDomainEvent(
    Guid TicketId,
    string? TicketNumber,
    Guid AssigneeId,
    Guid AssignedBy
) : INotification;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus OldStatus,
    TicketStatus NewStatus,
    Guid ChangedBy
) : INotification;

public sealed record TicketCommentAddedDomainEvent(
    Guid TicketId,
    Guid CommentId,
    string Body,
    IReadOnlyList<string> MentionedUsers
) : INotification;

public sealed record TicketResolvedDomainEvent(
    Guid TicketId,
    string? TicketNumber,
    Guid ResolvedBy
) : INotification;

public sealed record TicketClosedDomainEvent(
    Guid TicketId,
    string? TicketNumber,
    Guid ClosedBy
) : INotification;

public sealed record TicketReopenedDomainEvent(
    Guid TicketId,
    string? TicketNumber,
    Guid ReopenedBy
) : INotification;

public sealed record TicketMergedDomainEvent(
    Guid TicketId,
    string? TicketNumber,
    string MasterTicketNumber,
    Guid MergedBy
) : INotification;
