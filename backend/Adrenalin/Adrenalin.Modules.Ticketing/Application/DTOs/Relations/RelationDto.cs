namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record RelationDto
(
    Guid Id,
    Guid ParentTicketId,
    Guid ChildTicketId,
    string RelationType
);