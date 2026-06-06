namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record AssignTicketRequest(
    Guid AgentId,
    Guid AssignedBy,
    string? Notes
);