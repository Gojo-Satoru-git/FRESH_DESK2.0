using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Groups;

public sealed record GetGroupDashboardQuery(Guid GroupId, Guid ActorId)
    : IRequest<Result<GroupDashboardDto>>;

public sealed record GetGroupDashboardQueueQuery(
    Guid GroupId, string QueueType, Guid ActorId, int Page = 1, int PageSize = 20)
    : IRequest<Result<GroupQueueResultDto>>;

public sealed record GetLeadDashboardQuery(Guid LeadUserId)
    : IRequest<Result<LeadDashboardDto>>;

public sealed record GetGroupWorkloadQuery(Guid GroupId, Guid ActorId)
    : IRequest<Result<IReadOnlyList<GroupAgentWorkloadDto>>>;

