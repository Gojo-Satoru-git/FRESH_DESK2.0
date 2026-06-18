using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetGroupDashboardQuery(Guid GroupId, Guid ActorId)
    : IRequest<Result<GroupDashboardDto>>;

public sealed record GetGroupQueueQuery(
    Guid GroupId, string QueueType, Guid ActorId, int Page = 1, int PageSize = 20)
    : IRequest<Result<GroupQueueResultDto>>;

public sealed record GetLeadDashboardQuery(Guid LeadUserId)
    : IRequest<Result<LeadDashboardDto>>;

public sealed record GetGroupWorkloadQuery(Guid GroupId, Guid ActorId)
    : IRequest<Result<IReadOnlyList<GroupAgentWorkloadDto>>>;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public sealed record GroupAgentWorkloadDto(
    Guid AgentId,
    string AgentName,
    int ActiveTickets,
    int CriticalTickets,
    int OverdueTickets,
    int Capacity,
    int UtilizationPercent);

public sealed record GroupDashboardDto(
    Guid GroupId,
    string GroupName,
    int TotalTickets,
    int UnassignedCount,
    int AssignedCount,
    int OverdueCount,
    int CriticalCount,
    int EscalatedCount,
    IReadOnlyList<AgentWorkloadDto> AgentWorkloads);

public sealed record AgentWorkloadDto(
    Guid AgentId,
    string AgentName,
    int OpenTickets,
    int OverdueTickets);

public sealed record LeadDashboardDto(
    IReadOnlyList<GroupDashboardDto> Groups,
    int TotalUnassigned,
    int TotalOverdue,
    int TotalCritical);

public sealed record TicketQueueItemDto(
    Guid TicketId,
    string? TicketNumber,
    string Title,
    string Status,
    string Priority,
    string? CompanyName,
    DateTimeOffset CreatedAt,
    Guid? AssignedAgentId,
    string? AssignedAgentName,
    TimeSpan? Age);

public sealed record GroupQueueResultDto(
    IReadOnlyList<TicketQueueItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
