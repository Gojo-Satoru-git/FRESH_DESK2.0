namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record AssignTicketRequest(
    Guid AgentId,
    Guid AssignedBy,
    string? Notes
);