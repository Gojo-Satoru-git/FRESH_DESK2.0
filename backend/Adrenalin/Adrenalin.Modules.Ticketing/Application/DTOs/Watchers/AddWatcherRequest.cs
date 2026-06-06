namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record AddWatcherRequest
(
    Guid UserId,
    Guid AddedBy
);