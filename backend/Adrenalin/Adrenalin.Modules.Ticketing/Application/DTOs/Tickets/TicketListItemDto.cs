namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record TicketListItemDto
(
    Guid Id,
    string TicketNumber,
    string Title,
    string Status,
    string Priority,
    string DescriptionPreview,
    Guid? AssignedAgentId,
    Guid CompanyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);