using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

public sealed record RoutingRuleDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    Guid GroupId,
    string GroupName,
    Guid? ModuleId,
    string? ModuleName,
    string? RegionCode,
    string? TierCode,
    TicketPriority? Priority,
    TicketType? TicketType,
    string? Keywords,
    int RulePriority,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
