using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Queries;

public sealed record GetAllRolesQuery() : IRequest<Result<IReadOnlyList<RoleDto>>>;
public sealed record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDto>>;
public sealed record GetRoleWithPermissionsQuery(Guid RoleId) : IRequest<Result<RoleWithPermissionsDto>>;
public sealed record GetAllPermissionsQuery() : IRequest<Result<IReadOnlyList<PermissionDto>>>;
public sealed record GetPermissionsByRoleQuery(Guid RoleId) : IRequest<Result<IReadOnlyList<PermissionDto>>>;
public sealed record GetUserWithRolesQuery(Guid UserId) : IRequest<Result<UserWithRolesDto>>;
public sealed record GetUsersQuery(string? EmailQuery, bool? IsActive, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PagedResultDto<UserSummaryDto>>>;

/// <summary>
/// Returns all "resource:action" strings for a user by resolving their role chain.
/// Called at login time to stamp permissions into the JWT.
/// </summary>
public sealed record GetUserEffectivePermissionsQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<string>>>;

public sealed record GetAllGroupsQuery() : IRequest<Result<IReadOnlyList<GroupDto>>>;
public sealed record GetGroupByIdQuery(Guid GroupId) : IRequest<Result<GroupDto>>;
public sealed record GetGroupWithMembersQuery(Guid GroupId) : IRequest<Result<GroupWithMembersDto>>;
public sealed record GetUserGroupsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<GroupDto>>>;

public sealed record GetEnterpriseGroupMembersQuery(Guid GroupId)
    : IRequest<Result<IReadOnlyList<EnterpriseGroupMemberDto>>>;

public sealed record GetEnterpriseGroupLeadersQuery(Guid GroupId)
    : IRequest<Result<IReadOnlyList<EnterpriseGroupLeaderDto>>>;
