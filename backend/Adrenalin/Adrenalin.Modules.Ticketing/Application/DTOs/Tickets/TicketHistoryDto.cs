using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record TicketHistoryDto(
    IReadOnlyCollection<StatusHistoryDto> StatusHistory,
    IReadOnlyCollection<AssignmentLogDto> AssignmentLogs
);
