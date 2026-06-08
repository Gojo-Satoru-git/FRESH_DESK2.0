using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record GetTicketByIdResponse(
    Guid Id,
    string? TicketNumber,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid? AssignedAgentId,
    Guid? ReporterId,
    Guid CompanyId,
    string? Department,
    string? Region,
    string? ModuleName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<CommentDto> Comments,
    IReadOnlyCollection<StatusHistoryDto> StatusHistory,
    IReadOnlyCollection<AssignmentLogDto> AssignmentLogs,
    IReadOnlyCollection<WatcherDto> Watchers,
    IReadOnlyCollection<RelationDto> Relations,
    IReadOnlyCollection<AttachmentDto> Attachments,
    IReadOnlyCollection<TicketActivityDto> Activities,
    string? ReporterName = null,
    string? AssignedAgentName = null
);