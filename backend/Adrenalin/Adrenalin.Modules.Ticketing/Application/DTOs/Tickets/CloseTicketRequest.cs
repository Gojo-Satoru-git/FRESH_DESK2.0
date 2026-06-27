namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record CloseTicketRequest(
    Guid ClosedBy,
    string Notes
);
