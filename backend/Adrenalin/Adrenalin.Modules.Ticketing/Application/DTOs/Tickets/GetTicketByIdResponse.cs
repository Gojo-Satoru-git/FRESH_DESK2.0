using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record GetTicketByIdResponse(
    Guid Id,
    string? TicketNumber,
    string Subject,
    string Description,
    string Status,
    Guid? AssignedAgentId,
    Guid CompanyId,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<CommentDto> Comments,
    IReadOnlyCollection<StatusHistoryDto> StatusHistory,
    IReadOnlyCollection<AssignmentLogDto> AssignmentLogs,
    IReadOnlyCollection<WatcherDto> Watchers,
    IReadOnlyCollection<RelationDto> Relations,
    IReadOnlyCollection<AttachmentDto> Attachments
);