namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record MergeTicketRequest
(
    Guid DuplicateTicketId,
    Guid MergedBy
);