namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record CloseTicketRequest(
    Guid ClosedBy,
    string Notes
);
