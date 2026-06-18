using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Groups;

// ═══ GROUP DASHBOARD ═════════════════════════════════════════════════════════

public sealed class GetGroupDashboardQueryHandler
    : IRequestHandler<GetGroupDashboardQuery, Result<GroupDashboardDto>>
{
    private readonly ITicketDashboardRepository _dashboardRepo;

    public GetGroupDashboardQueryHandler(ITicketDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<Result<GroupDashboardDto>> Handle(
        GetGroupDashboardQuery query, CancellationToken ct)
    {
        try
        {
            var hasAccess = await _dashboardRepo.IsGroupMemberOrAdminAsync(query.GroupId, query.ActorId, ct);
            if (!hasAccess)
                return Result<GroupDashboardDto>.Failure("Access denied. You are not a member of this group.");

            var dashboard = await _dashboardRepo.GetGroupDashboardDataAsync(query.GroupId, query.ActorId, ct);
            if (dashboard is null)
                return Result<GroupDashboardDto>.Failure($"Group {query.GroupId} not found.");

            return Result<GroupDashboardDto>.Success(dashboard);
        }
        catch (Exception ex) { return Result<GroupDashboardDto>.Failure(ex.Message); }
    }
}

// ═══ GROUP QUEUE ═════════════════════════════════════════════════════════════

public sealed class GetGroupQueueQueryHandler
    : IRequestHandler<GetGroupQueueQuery, Result<GroupQueueResultDto>>
{
    private readonly ITicketDashboardRepository _dashboardRepo;

    public GetGroupQueueQueryHandler(ITicketDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<Result<GroupQueueResultDto>> Handle(
        GetGroupQueueQuery query, CancellationToken ct)
    {
        try
        {
            var hasAccess = await _dashboardRepo.IsGroupMemberOrAdminAsync(query.GroupId, query.ActorId, ct);
            if (!hasAccess)
                return Result<GroupQueueResultDto>.Failure("Access denied. You are not a member of this group.");

            var queue = await _dashboardRepo.GetGroupQueueDataAsync(query.GroupId, query.QueueType, query.ActorId, query.Page, query.PageSize, ct);
            if (queue is null)
                return Result<GroupQueueResultDto>.Failure($"Group {query.GroupId} not found.");

            return Result<GroupQueueResultDto>.Success(queue);
        }
        catch (Exception ex) { return Result<GroupQueueResultDto>.Failure(ex.Message); }
    }
}

// ═══ LEAD DASHBOARD (MULTI-GROUP) ═══════════════════════════════════════════

public sealed class GetLeadDashboardQueryHandler
    : IRequestHandler<GetLeadDashboardQuery, Result<LeadDashboardDto>>
{
    private readonly ITicketDashboardRepository _dashboardRepo;

    public GetLeadDashboardQueryHandler(ITicketDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<Result<LeadDashboardDto>> Handle(
        GetLeadDashboardQuery query, CancellationToken ct)
    {
        try
        {
            var dashboard = await _dashboardRepo.GetLeadDashboardDataAsync(query.LeadUserId, ct);
            if (dashboard is null)
                return Result<LeadDashboardDto>.Failure("User is not a lead of any active groups.");

            return Result<LeadDashboardDto>.Success(dashboard);
        }
        catch (Exception ex) { return Result<LeadDashboardDto>.Failure(ex.Message); }
    }
}

// ═══ GROUP WORKLOAD ══════════════════════════════════════════════════════════

public sealed class GetGroupWorkloadQueryHandler
    : IRequestHandler<GetGroupWorkloadQuery, Result<IReadOnlyList<GroupAgentWorkloadDto>>>
{
    private readonly ITicketDashboardRepository _dashboardRepo;

    public GetGroupWorkloadQueryHandler(ITicketDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<Result<IReadOnlyList<GroupAgentWorkloadDto>>> Handle(
        GetGroupWorkloadQuery query, CancellationToken ct)
    {
        try
        {
            var isAuthorized = await _dashboardRepo.IsGroupMemberOrAdminAsync(query.GroupId, query.ActorId, ct);
            if (!isAuthorized)
            {
                return Result<IReadOnlyList<GroupAgentWorkloadDto>>.Failure("User is not authorized to view this group's workload.");
            }

            var workload = await _dashboardRepo.GetGroupWorkloadAsync(query.GroupId, ct);
            if (workload is null)
                return Result<IReadOnlyList<GroupAgentWorkloadDto>>.Failure($"Group {query.GroupId} not found.");

            return Result<IReadOnlyList<GroupAgentWorkloadDto>>.Success(workload);
        }
        catch (Exception ex) { return Result<IReadOnlyList<GroupAgentWorkloadDto>>.Failure(ex.Message); }
    }
}
