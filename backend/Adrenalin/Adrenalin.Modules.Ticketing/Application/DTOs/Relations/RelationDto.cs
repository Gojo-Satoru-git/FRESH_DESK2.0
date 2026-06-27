namespace Adrenalin.Modules.Ticketing.Application.DTOs.Relations;

public sealed record RelationDto
(
    Guid Id,
    Guid ParentTicketId,
    Guid ChildTicketId,
    string RelationType
);