namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record TicketDashboardDto(
    int totalTickets,
    int TotalActive,
    int InProgress,
    int PendingReply,
    int ResolvedClosed
);
