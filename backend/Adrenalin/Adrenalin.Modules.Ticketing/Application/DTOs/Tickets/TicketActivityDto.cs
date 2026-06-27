using System;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record TicketActivityDto(
    Guid Id,
    string ActivityType,
    string? OldValue,
    string? NewValue,
    Guid? PerformedBy,
    DateTimeOffset PerformedAt,
    string? PerformedByName = null
);
