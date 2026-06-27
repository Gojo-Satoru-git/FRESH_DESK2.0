namespace Adrenalin.Modules.Ticketing.Application.DTOs.Relations;

public sealed record MergeTicketRequest
(
    Guid DuplicateTicketId,
    Guid MergedBy
);