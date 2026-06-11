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
    string Type,
    Guid? AssignedAgentId,
    Guid? ReporterId,
    Guid CompanyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<CommentDto> Comments,
    IReadOnlyCollection<StatusHistoryDto> StatusHistory,
    IReadOnlyCollection<AssignmentLogDto> AssignmentLogs,
    IReadOnlyCollection<AttachmentDto> Attachments,
    string? ReporterName = null,
    string? AssignedAgentName = null
);