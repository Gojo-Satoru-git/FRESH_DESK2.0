using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

// ═══ ROLE ════════════════════════════════════════════════════════════════════

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IRoleRepository _roles;
    public CreateRoleCommandHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<Guid>> Handle(CreateRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _roles.ExistsByNameAsync(cmd.Name, ct))
                return Result<Guid>.Failure($"A role named '{cmd.Name}' already exists.");
            var role = Role.Create(cmd.Name, cmd.Description, cmd.ActorId);
            _roles.Add(role);
           
            return Result<Guid>.Success(role.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    public UpdateRoleCommandHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result> Handle(UpdateRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var role = await _roles.GetByIdAsync(cmd.RoleId, ct);
            if (role is null) return Result.Failure($"Role {cmd.RoleId} not found.");
            if (!role.Name.Equals(cmd.Name, StringComparison.OrdinalIgnoreCase)
                && await _roles.ExistsByNameAsync(cmd.Name, ct))
                return Result.Failure($"A role named '{cmd.Name}' already exists.");
            role.Update(cmd.Name, cmd.Description, cmd.ActorId);
            _roles.Update(role);
            await _roles.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    public DeleteRoleCommandHandler(IRoleRepository roles, IUserRoleRepository userRoles)
        => (_roles, _userRoles) = (roles, userRoles);

    public async Task<Result> Handle(DeleteRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var role = await _roles.GetByIdAsync(cmd.RoleId, ct);
            if (role is null) return Result.Failure($"Role {cmd.RoleId} not found.");
            role.SoftDelete(cmd.ActorId);
            await _userRoles.SoftDeleteByRoleAsync(cmd.RoleId, cmd.ActorId, ct);
            _roles.Update(role);
            await _roles.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ PERMISSION ══════════════════════════════════════════════════════════════

public sealed class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    private readonly IPermissionRepository _perms;
    public CreatePermissionCommandHandler(IPermissionRepository perms) => _perms = perms;

    public async Task<Result<Guid>> Handle(CreatePermissionCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _perms.ExistsAsync(cmd.Resource, cmd.Action, ct))
                return Result<Guid>.Failure($"Permission '{cmd.Resource}:{cmd.Action}' already exists.");
            var perm = Permission.Create(cmd.Resource, cmd.Action, cmd.Description, cmd.ActorId);
            _perms.Add(perm);
            await _perms.SaveChangesAsync(ct);
            return Result<Guid>.Success(perm.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

public sealed class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
{
    private readonly IPermissionRepository _perms;
    private readonly IRolePermissionRepository _rp;
    public DeletePermissionCommandHandler(IPermissionRepository perms, IRolePermissionRepository rp)
        => (_perms, _rp) = (perms, rp);

    public async Task<Result> Handle(DeletePermissionCommand cmd, CancellationToken ct)
    {
        try
        {
            var perm = await _perms.GetByIdAsync(cmd.PermissionId, ct);
            if (perm is null) return Result.Failure($"Permission {cmd.PermissionId} not found.");
            await _rp.SoftDeleteByPermissionAsync(cmd.PermissionId, cmd.ActorId, ct);
            perm.SoftDelete(cmd.ActorId);
            _perms.Update(perm);
            await _perms.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ ROLE ↔ PERMISSION ═══════════════════════════════════════════════════════

public sealed class GrantPermissionToRoleCommandHandler : IRequestHandler<GrantPermissionToRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IPermissionRepository _perms;
    private readonly IRolePermissionRepository _rp;
    public GrantPermissionToRoleCommandHandler(IRoleRepository roles, IPermissionRepository perms,
        IRolePermissionRepository rp) => (_roles, _perms, _rp) = (roles, perms, rp);

    public async Task<Result> Handle(GrantPermissionToRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _roles.GetByIdAsync(cmd.RoleId, ct) is null)
                return Result.Failure($"Role {cmd.RoleId} not found.");
            if (await _perms.GetByIdAsync(cmd.PermissionId, ct) is null)
                return Result.Failure($"Permission {cmd.PermissionId} not found.");
            var existing = await _rp.GetAsync(cmd.RoleId, cmd.PermissionId, ct);
            if (existing is not null) return Result.Success(); // idempotent
            _rp.Add(RolePermission.Assign(cmd.RoleId, cmd.PermissionId, cmd.ActorId));
            await _rp.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class RevokePermissionFromRoleCommandHandler : IRequestHandler<RevokePermissionFromRoleCommand, Result>
{
    private readonly IRolePermissionRepository _rp;
    public RevokePermissionFromRoleCommandHandler(IRolePermissionRepository rp) => _rp = rp;

    public async Task<Result> Handle(RevokePermissionFromRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var rp = await _rp.GetAsync(cmd.RoleId, cmd.PermissionId, ct);
            if (rp is null) return Result.Failure("Permission is not assigned to this role.");
            rp.SoftDelete(cmd.ActorId);
            _rp.Update(rp);
            await _rp.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class SetRolePermissionsCommandHandler : IRequestHandler<SetRolePermissionsCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IPermissionRepository _perms;
    private readonly IRolePermissionRepository _rp;
    public SetRolePermissionsCommandHandler(IRoleRepository roles, IPermissionRepository perms,
        IRolePermissionRepository rp) => (_roles, _perms, _rp) = (roles, perms, rp);

    public async Task<Result> Handle(SetRolePermissionsCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _roles.GetByIdAsync(cmd.RoleId, ct) is null)
                return Result.Failure($"Role {cmd.RoleId} not found.");
            foreach (var permId in cmd.PermissionIds)
                if (await _perms.GetByIdAsync(permId, ct) is null)
                    return Result.Failure($"Permission {permId} not found.");
            await _rp.SoftDeleteByRoleAsync(cmd.RoleId, cmd.ActorId, ct);
            foreach (var permId in cmd.PermissionIds)
                _rp.Add(RolePermission.Assign(cmd.RoleId, permId, cmd.ActorId));
            await _rp.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ USER ↔ ROLE ═════════════════════════════════════════════════════════════

public sealed class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _ur;
    public AssignRoleToUserCommandHandler(IUserRepository users, IRoleRepository roles, IUserRoleRepository ur)
        => (_users, _roles, _ur) = (users, roles, ur);

    public async Task<Result> Handle(AssignRoleToUserCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _users.GetByIdAsync(cmd.UserId, ct) is null)
                return Result.Failure($"User {cmd.UserId} not found.");
            if (await _roles.GetByIdAsync(cmd.RoleId, ct) is null)
                return Result.Failure($"Role {cmd.RoleId} not found.");
            if (await _ur.GetAsync(cmd.UserId, cmd.RoleId, ct) is not null)
                return Result.Success(); // idempotent
            var deleted = await _ur.GetIncludingDeletedAsync(cmd.UserId, cmd.RoleId, ct);
            if (deleted is not null) { deleted.Restore(cmd.ActorId); _ur.Update(deleted); }
            else _ur.Add(UserRole.Assign(cmd.UserId, cmd.RoleId, cmd.ActorId));
            await _ur.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IUserRoleRepository _ur;
    public RemoveRoleFromUserCommandHandler(IUserRoleRepository ur) => _ur = ur;

    public async Task<Result> Handle(RemoveRoleFromUserCommand cmd, CancellationToken ct)
    {
        try
        {
            var ur = await _ur.GetAsync(cmd.UserId, cmd.RoleId, ct);
            if (ur is null) return Result.Failure("Role is not assigned to this user.");
            ur.SoftDelete(cmd.ActorId);
            _ur.Update(ur);
            await _ur.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand, Result>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _ur;
    public SetUserRolesCommandHandler(IUserRepository users, IRoleRepository roles, IUserRoleRepository ur)
        => (_users, _roles, _ur) = (users, roles, ur);

    public async Task<Result> Handle(SetUserRolesCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _users.GetByIdAsync(cmd.UserId, ct) is null)
                return Result.Failure($"User {cmd.UserId} not found.");
            foreach (var roleId in cmd.RoleIds)
                if (await _roles.GetByIdAsync(roleId, ct) is null)
                    return Result.Failure($"Role {roleId} not found.");
            await _ur.SoftDeleteByUserAsync(cmd.UserId, cmd.ActorId, ct);
            foreach (var roleId in cmd.RoleIds)
                _ur.Add(UserRole.Assign(cmd.UserId, roleId, cmd.ActorId));
            await _ur.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ GROUP ═══════════════════════════════════════════════════════════════════

public sealed class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Result<Guid>>
{
    private readonly IGroupRepository _groups;
    public CreateGroupCommandHandler(IGroupRepository groups) => _groups = groups;

    public async Task<Result<Guid>> Handle(CreateGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _groups.ExistsByNameAsync(cmd.Name, ct))
                return Result<Guid>.Failure($"A group named '{cmd.Name}' already exists.");
            var group = Group.Create(cmd.Name, cmd.RegionCode, cmd.TierCode, cmd.UnattendedAlertMinutes, cmd.ActorId);
            _groups.Add(group);
            await _groups.SaveChangesAsync(ct);
            return Result<Guid>.Success(group.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

public sealed class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, Result>
{
    private readonly IGroupRepository _groups;
    public UpdateGroupCommandHandler(IGroupRepository groups) => _groups = groups;

    public async Task<Result> Handle(UpdateGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var group = await _groups.GetByIdAsync(cmd.GroupId, ct);
            if (group is null) return Result.Failure($"Group {cmd.GroupId} not found.");
            if (!group.Name.Equals(cmd.Name, StringComparison.OrdinalIgnoreCase)
                && await _groups.ExistsByNameAsync(cmd.Name, ct))
                return Result.Failure($"A group named '{cmd.Name}' already exists.");
            group.Update(cmd.Name, cmd.RegionCode, cmd.TierCode, cmd.UnattendedAlertMinutes, cmd.ActorId);
            _groups.Update(group);
            await _groups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, Result>
{
    private readonly IGroupRepository _groups;
    private readonly IUserGroupRepository _ug;
    public DeleteGroupCommandHandler(IGroupRepository groups, IUserGroupRepository ug)
        => (_groups, _ug) = (groups, ug);

    public async Task<Result> Handle(DeleteGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var group = await _groups.GetByIdAsync(cmd.GroupId, ct);
            if (group is null) return Result.Failure($"Group {cmd.GroupId} not found.");
            group.SoftDelete(cmd.ActorId);
            await _ug.SoftDeleteByGroupAsync(cmd.GroupId, cmd.ActorId, ct);
            _groups.Update(group);
            await _groups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ USER ↔ GROUP ════════════════════════════════════════════════════════════

public sealed class AddUserToGroupCommandHandler : IRequestHandler<AddUserToGroupCommand, Result>
{
    private readonly IUserRepository _users;
    private readonly IGroupRepository _groups;
    private readonly IUserGroupRepository _ug;
    public AddUserToGroupCommandHandler(IUserRepository users, IGroupRepository groups, IUserGroupRepository ug)
        => (_users, _groups, _ug) = (users, groups, ug);

    public async Task<Result> Handle(AddUserToGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            if (await _users.GetByIdAsync(cmd.UserId, ct) is null)
                return Result.Failure($"User {cmd.UserId} not found.");
            if (await _groups.GetByIdAsync(cmd.GroupId, ct) is null)
                return Result.Failure($"Group {cmd.GroupId} not found.");
            if (await _ug.GetAsync(cmd.UserId, cmd.GroupId, ct) is not null)
                return Result.Success(); // idempotent
            var deleted = await _ug.GetIncludingDeletedAsync(cmd.UserId, cmd.GroupId, ct);
            if (deleted is not null) { deleted.Restore(cmd.IsLead, cmd.ActorId); _ug.Update(deleted); }
            else _ug.Add(UserGroup.Add(cmd.UserId, cmd.GroupId, cmd.IsLead, cmd.ActorId));
            await _ug.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class RemoveUserFromGroupCommandHandler : IRequestHandler<RemoveUserFromGroupCommand, Result>
{
    private readonly IUserGroupRepository _ug;
    public RemoveUserFromGroupCommandHandler(IUserGroupRepository ug) => _ug = ug;

    public async Task<Result> Handle(RemoveUserFromGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var ug = await _ug.GetAsync(cmd.UserId, cmd.GroupId, ct);
            if (ug is null) return Result.Failure("User is not a member of this group.");
            ug.SoftDelete(cmd.ActorId);
            _ug.Update(ug);
            await _ug.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class SetGroupLeadCommandHandler : IRequestHandler<SetGroupLeadCommand, Result>
{
    private readonly IUserGroupRepository _ug;
    public SetGroupLeadCommandHandler(IUserGroupRepository ug) => _ug = ug;

    public async Task<Result> Handle(SetGroupLeadCommand cmd, CancellationToken ct)
    {
        try
        {
            var ug = await _ug.GetAsync(cmd.UserId, cmd.GroupId, ct);
            if (ug is null) return Result.Failure("User is not a member of this group.");
            ug.SetLead(cmd.IsLead, cmd.ActorId);
            _ug.Update(ug);
            await _ug.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
