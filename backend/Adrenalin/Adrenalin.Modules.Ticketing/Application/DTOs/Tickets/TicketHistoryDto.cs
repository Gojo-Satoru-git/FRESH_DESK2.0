using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record TicketHistoryDto(
    IReadOnlyCollection<StatusHistoryDto> StatusHistory,
    IReadOnlyCollection<AssignmentLogDto> AssignmentLogs
);
