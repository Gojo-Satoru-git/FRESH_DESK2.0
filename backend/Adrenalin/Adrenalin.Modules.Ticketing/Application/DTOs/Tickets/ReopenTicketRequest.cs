namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record ReopenTicketRequest(
    Guid ReopenedBy,
    string Reason
);
