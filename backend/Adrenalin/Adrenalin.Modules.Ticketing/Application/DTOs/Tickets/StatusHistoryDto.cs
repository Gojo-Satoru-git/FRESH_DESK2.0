namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record StatusHistoryDto(
    Guid Id,
    string? FromStatus,
    string ToStatus,
    Guid? ChangedBy,
    string? Reason,
    DateTimeOffset CreatedAt
);