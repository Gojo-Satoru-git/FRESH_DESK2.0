namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record RoutingRuleHistoryDto(
    Guid Id,
    Guid RuleId,
    string Action,
    string? OldValues,
    string? NewValues,
    Guid? ChangedBy,
    string? ChangedByName,
    DateTime ChangedAt,
    string? IpAddress);
