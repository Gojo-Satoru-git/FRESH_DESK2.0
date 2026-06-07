using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    private readonly IRoleRepository _roles;
    public GetAllRolesQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetAllRolesQuery q, CancellationToken ct)
    {
        var roles = await _roles.GetAllAsync(ct);
        return Result<IReadOnlyList<RoleDto>>.Success(roles
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsSystemRole, r.CreatedAt, r.UpdatedAt))
            .ToList());
    }
}

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IRoleRepository _roles;
    public GetRoleByIdQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery q, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(q.RoleId, ct);
        if (role is null) return Result<RoleDto>.Failure($"Role {q.RoleId} not found.");
        return Result<RoleDto>.Success(new RoleDto(role.Id, role.Name, role.Description,
            role.IsSystemRole, role.CreatedAt, role.UpdatedAt));
    }
}

public sealed class GetRoleWithPermissionsQueryHandler
    : IRequestHandler<GetRoleWithPermissionsQuery, Result<RoleWithPermissionsDto>>
{
    private readonly IRoleRepository _roles;
    public GetRoleWithPermissionsQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<RoleWithPermissionsDto>> Handle(GetRoleWithPermissionsQuery q, CancellationToken ct)
    {
        var role = await _roles.GetWithPermissionsAsync(q.RoleId, ct);
        if (role is null) return Result<RoleWithPermissionsDto>.Failure($"Role {q.RoleId} not found.");
        var permissions = role.RolePermissions
            .Where(rp => !rp.IsDeleted)
            .Select(rp => new PermissionDto(rp.Permission.Id, rp.Permission.Resource,
                rp.Permission.Action, rp.Permission.Description))
            .ToList();
        return Result<RoleWithPermissionsDto>.Success(new RoleWithPermissionsDto(role.Id, role.Name,
            role.Description, role.IsSystemRole, permissions, role.CreatedAt, role.UpdatedAt));
    }
}

public sealed class GetAllPermissionsQueryHandler
    : IRequestHandler<GetAllPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IPermissionRepository _perms;
    public GetAllPermissionsQueryHandler(IPermissionRepository perms) => _perms = perms;

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(GetAllPermissionsQuery q, CancellationToken ct)
    {
        var perms = await _perms.GetAllAsync(ct);
        return Result<IReadOnlyList<PermissionDto>>.Success(perms
            .Select(p => new PermissionDto(p.Id, p.Resource, p.Action, p.Description))
            .ToList());
    }
}

public sealed class GetPermissionsByRoleQueryHandler
    : IRequestHandler<GetPermissionsByRoleQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IRolePermissionRepository _rp;
    public GetPermissionsByRoleQueryHandler(IRolePermissionRepository rp) => _rp = rp;

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(GetPermissionsByRoleQuery q, CancellationToken ct)
    {
        var rows = await _rp.GetByRoleWithPermissionsAsync(q.RoleId, ct);
        return Result<IReadOnlyList<PermissionDto>>.Success(rows
            .Select(rp => new PermissionDto(rp.Permission.Id, rp.Permission.Resource,
                rp.Permission.Action, rp.Permission.Description))
            .ToList());
    }
}

public sealed class GetUserWithRolesQueryHandler
    : IRequestHandler<GetUserWithRolesQuery, Result<UserWithRolesDto>>
{
    private readonly IUserRepository _users;
    public GetUserWithRolesQueryHandler(IUserRepository users) => _users = users;

    public async Task<Result<UserWithRolesDto>> Handle(GetUserWithRolesQuery q, CancellationToken ct)
    {
        var user = await _users.GetWithRolesAsync(q.UserId, ct);
        if (user is null) return Result<UserWithRolesDto>.Failure($"User {q.UserId} not found.");
        var roles = user.UserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => new RoleSummaryDto(ur.Role.Id, ur.Role.Name, ur.Role.IsSystemRole))
            .ToList();
        return Result<UserWithRolesDto>.Success(new UserWithRolesDto(user.Id, user.Email,
            user.FirstName, user.LastName, user.IsActive, roles, user.CreatedAt, user.LastLoginAt));
    }
}

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<PagedResultDto<UserSummaryDto>>>
{
    private readonly IUserRepository _users;
    public GetUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<Result<PagedResultDto<UserSummaryDto>>> Handle(GetUsersQuery q, CancellationToken ct)
    {
        var (items, total) = await _users.GetPagedAsync(q.EmailQuery, q.IsActive, q.PageNumber, q.PageSize, ct);
        var dtos = items.Select(u => new UserSummaryDto(u.Id, u.Email, u.FirstName, u.LastName, u.IsActive)).ToList();
        return Result<PagedResultDto<UserSummaryDto>>.Success(
            new PagedResultDto<UserSummaryDto>(dtos, total, q.PageNumber, q.PageSize));
    }
}

public sealed class GetUserEffectivePermissionsQueryHandler
    : IRequestHandler<GetUserEffectivePermissionsQuery, Result<IReadOnlyList<string>>>
{
    private readonly IUserRepository _users;
    public GetUserEffectivePermissionsQueryHandler(IUserRepository users) => _users = users;

    public async Task<Result<IReadOnlyList<string>>> Handle(GetUserEffectivePermissionsQuery q, CancellationToken ct)
    {
        var permissions = await _users.GetEffectivePermissionsAsync(q.UserId, ct);
        return Result<IReadOnlyList<string>>.Success(permissions);
    }
}

public sealed class GetAllGroupsQueryHandler
    : IRequestHandler<GetAllGroupsQuery, Result<IReadOnlyList<GroupDto>>>
{
    private readonly IGroupRepository _groups;
    public GetAllGroupsQueryHandler(IGroupRepository groups) => _groups = groups;

    public async Task<Result<IReadOnlyList<GroupDto>>> Handle(GetAllGroupsQuery q, CancellationToken ct)
    {
        var groups = await _groups.GetAllAsync(ct);
        return Result<IReadOnlyList<GroupDto>>.Success(groups.Select(g => g.ToDto()).ToList());
    }
}

public sealed class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, Result<GroupDto>>
{
    private readonly IGroupRepository _groups;
    public GetGroupByIdQueryHandler(IGroupRepository groups) => _groups = groups;

    public async Task<Result<GroupDto>> Handle(GetGroupByIdQuery q, CancellationToken ct)
    {
        var g = await _groups.GetByIdAsync(q.GroupId, ct);
        if (g is null) return Result<GroupDto>.Failure($"Group {q.GroupId} not found.");
        return Result<GroupDto>.Success(g.ToDto());
    }
}

public sealed class GetGroupWithMembersQueryHandler
    : IRequestHandler<GetGroupWithMembersQuery, Result<GroupWithMembersDto>>
{
    private readonly IGroupRepository _groups;
    public GetGroupWithMembersQueryHandler(IGroupRepository groups) => _groups = groups;

    public async Task<Result<GroupWithMembersDto>> Handle(GetGroupWithMembersQuery q, CancellationToken ct)
    {
        var group = await _groups.GetWithMembersAsync(q.GroupId, ct);
        if (group is null) return Result<GroupWithMembersDto>.Failure($"Group {q.GroupId} not found.");
        var members = group.UserGroups.Where(ug => !ug.IsDeleted)
            .Select(ug => new GroupMemberDto(ug.User.Id, ug.User.Email,
                ug.User.FirstName, ug.User.LastName, ug.IsLead)).ToList();
        return Result<GroupWithMembersDto>.Success(new GroupWithMembersDto(group.ToDto(), members));
    }
}

public sealed class GetUserGroupsQueryHandler
    : IRequestHandler<GetUserGroupsQuery, Result<IReadOnlyList<GroupDto>>>
{
    private readonly IUserGroupRepository _ug;
    private readonly IGroupRepository _groups;
    public GetUserGroupsQueryHandler(IUserGroupRepository ug, IGroupRepository groups)
        => (_ug, _groups) = (ug, groups);

    public async Task<Result<IReadOnlyList<GroupDto>>> Handle(GetUserGroupsQuery q, CancellationToken ct)
    {
        var memberships = await _ug.GetByUserAsync(q.UserId, ct);
        var result = new List<GroupDto>();
        foreach (var m in memberships)
        {
            var g = await _groups.GetByIdAsync(m.GroupId, ct);
            if (g is not null) result.Add(g.ToDto());
        }
        return Result<IReadOnlyList<GroupDto>>.Success(result);
    }
}

// ── Shared mapping helper ─────────────────────────────────────────────────────
file static class GroupMapper
{
    internal static GroupDto ToDto(Adrenalin.Modules.Auth.Domain.Entities.Group g) =>
        new(g.Id, g.Name, g.RegionCode, g.TierCode, g.UnattendedAlertMinutes,
            g.IsActive, g.CreatedAt, g.UpdatedAt);
}

// Make ToDto available in file scope
file static class FileExtensions
{
    internal static GroupDto ToDto(this Adrenalin.Modules.Auth.Domain.Entities.Group g)
        => GroupMapper.ToDto(g);
}
