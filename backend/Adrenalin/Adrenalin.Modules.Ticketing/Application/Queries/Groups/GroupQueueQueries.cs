using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Groups;

/// <summary>
/// The dispatch queue for one group — every ticket with GroupId set and
/// AssignedAgentId null. CallerId is required so the handler can verify the
/// caller is allowed to see it (group lead, group member, or platform admin —
/// see GroupQueueQueryHandler for the exact rule).
/// </summary>
public sealed record GetGroupQueueQuery(
    Guid GroupId,
    Guid CallerId) : IRequest<Result<GroupQueueSummaryDto>>;

/// <summary>
/// Combined queue across every group the caller leads. A user can be
/// IsLead = true on more than one UserGroup row (one row per group), so this
/// naturally covers "team lead in multiple groups" without any schema change —
/// see UserGroup.IsLead and IUserGroupRepository.GetByUserAsync.
/// </summary>
public sealed record GetMyLeadGroupQueuesQuery(
    Guid CallerId) : IRequest<Result<IReadOnlyList<GroupQueueSummaryDto>>>;
