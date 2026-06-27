namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record TicketDashboardDto(
    int TotalTickets,
    int TotalActive,
    int InProgress,
    int PendingReply,
    int ResolvedClosed,
    TicketCountsDto Counts,
    PerformanceMetricsDto Performance,
    IReadOnlyList<TrendDataDto> Trends,
    IReadOnlyList<DashboardAgentWorkloadDto> AgentWorkloads,
    IReadOnlyList<GroupMetricDto> GroupMetrics,
    IReadOnlyList<TodoTaskDto> Todos,
    SlaMetricsDto SlaMetrics
);

public sealed record TicketCountsDto(
    int Total,
    int Unassigned,
    int Open,
    int Pending
);

public sealed record PerformanceMetricsDto(
    int ReceivedToday,
    int ResolvedToday,
    int? ResolutionRate
);

public sealed record TrendDataDto(
    string TimeLabel,
    int TodayCount,
    int YesterdayCount
);

public sealed record TodoTaskDto(
    string Id,
    string Title,
    string Due
);

public sealed record DashboardAgentWorkloadDto(
    System.Guid AgentId,
    string AgentName,
    int OpenTickets,
    int OverdueTickets
);

public sealed record GroupMetricDto(
    System.Guid GroupId,
    string GroupName,
    int TicketCount
);

public sealed record SlaMetricsDto(
    int BreachedCount,
    int AtRiskCount
);
