namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record TicketListItemDto
(
    Guid Id,
    string TicketNumber,
    string Subject,
    string Status,
    Guid? AssignedAgentId,
    Guid CompanyId,
    DateTimeOffset CreatedAt
);