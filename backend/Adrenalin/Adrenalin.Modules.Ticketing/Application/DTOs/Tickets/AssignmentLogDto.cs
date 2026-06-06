namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record AssignmentLogDto(
    Guid Id,
    Guid? FromAgentId,
    Guid ToAgentId,
    Guid? ChangedBy,
    string? Notes,
    DateTimeOffset CreatedAt
);