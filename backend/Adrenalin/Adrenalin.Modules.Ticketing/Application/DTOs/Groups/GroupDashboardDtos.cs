namespace Adrenalin.Modules.Ticketing.Application.DTOs.Groups;

public sealed record GroupAgentWorkloadDto(
    Guid AgentId,
    string AgentName,
    int ActiveTickets,
    int CriticalTickets,
    int OverdueTickets,
    int Capacity,
    int UtilizationPercent);

public sealed record AgentWorkloadDto(
    Guid AgentId,
    string AgentName,
    int OpenTickets,
    int OverdueTickets);

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