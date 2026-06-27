namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record CompanyTicketMetricsDto(
    Guid CompanyId,
    int TotalTickets,
    int UnassignedTickets,
    int OpenTickets,
    int CriticalTickets,
    int SlaBreachedTickets,
    int ResolvedTickets
);
