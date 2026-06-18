using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

// ── Routing Rules ─────────────────────────────────────────────────────────────

public sealed record CreateRoutingRuleCommand(
    Guid CompanyId,
    Guid GroupId,
    Guid? ModuleId,
    string? RegionCode,
    string? TierCode,
    TicketPriority? Priority,
    TicketType? TicketType,
    string? Keywords,
    int RulePriority,
    bool IsDefault,
    Guid ActorId) : IRequest<Result<Guid>>;

public sealed record UpdateRoutingRuleCommand(
    Guid RuleId,
    Guid GroupId,
    Guid? ModuleId,
    string? RegionCode,
    string? TierCode,
    TicketPriority? Priority,
    TicketType? TicketType,
    string? Keywords,
    int RulePriority,
    bool IsDefault,
    Guid ActorId) : IRequest<Result>;

public sealed record DeleteRoutingRuleCommand(
    Guid RuleId,
    Guid ActorId) : IRequest<Result>;

// ── Ticket Group Routing ──────────────────────────────────────────────────────

public sealed record AssignTicketToGroupCommand(
    Guid TicketId,
    Guid GroupId,
    Guid ActorId,
    string? Reason = null) : IRequest<Result>;

public sealed record AutoRouteTicketCommand(
    Guid TicketId,
    Guid ActorId) : IRequest<Result<RoutingResultDto>>;

public sealed record RoutingResultDto(
    Guid? GroupId,
    Guid? AgentId,
    string MatchedRule,
    string? RuleDescription);

public sealed record SimulateRoutingCommand(
    Guid CompanyId,
    Guid? ModuleId,
    TicketPriority? Priority,
    TicketType? Type,
    string? Title,
    string? Description) : IRequest<Result<Adrenalin.Modules.Ticketing.Application.DTOs.RoutingSimulationResultDto>>;
