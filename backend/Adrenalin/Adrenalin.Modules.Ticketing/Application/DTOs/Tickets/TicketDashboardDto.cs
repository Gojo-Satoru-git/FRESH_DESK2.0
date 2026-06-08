namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record TicketDashboardDto(
    int TotalActive,
    int InProgress,
    int PendingReply,
    int ResolvedClosed
);
