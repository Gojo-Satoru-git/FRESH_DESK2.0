namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record ReopenTicketRequest(
    Guid ReopenedBy,
    string Reason
);
