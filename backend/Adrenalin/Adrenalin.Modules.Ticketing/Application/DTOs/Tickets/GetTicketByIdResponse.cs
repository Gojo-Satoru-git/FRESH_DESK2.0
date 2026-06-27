using System;
using System.Collections.Generic;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

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