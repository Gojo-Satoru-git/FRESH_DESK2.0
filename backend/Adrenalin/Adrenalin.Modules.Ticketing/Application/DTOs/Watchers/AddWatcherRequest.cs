namespace Adrenalin.Modules.Ticketing.Application.DTOs.Watchers;

public sealed record AddWatcherRequest
(
    Guid UserId,
    Guid AddedBy
);