using System;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record WatcherDto(
    Guid Id,
    Guid UserId,
    DateTimeOffset AddedAt
);