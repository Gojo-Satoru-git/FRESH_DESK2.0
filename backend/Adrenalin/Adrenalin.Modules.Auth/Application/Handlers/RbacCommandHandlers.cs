using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Auth.Domain.Services;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;

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
            await _roles.SaveChangesAsync(ct);
           
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
    private readonly IAuditLogWriter _audit;

    public DeleteRoleCommandHandler(IRoleRepository roles, IUserRoleRepository userRoles, IAuditLogWriter audit)
        => (_roles, _userRoles, _audit) = (roles, userRoles, audit);

    public async Task<Result> Handle(DeleteRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var role = await _roles.GetByIdAsync(cmd.RoleId, ct);
            if (role is null) return Result.Failure($"Role {cmd.RoleId} not found.");

            // FR-RP-025 — must be Inactive first. (Requires Role.IsActive from PATCH 26.)
            if (role.IsActive)
                return Result.Failure("Access Level must be deactivated before it can be deleted.");

            // BR-RP-005 — held by zero agents, active or inactive.
            var holderCount = await _userRoles.CountByRoleAsync(cmd.RoleId, ct);
            if (holderCount > 0)
                return Result.Failure(
                    $"Cannot delete: {holderCount} agent(s) currently hold this Access Level. Reassign them first.");

            role.SoftDelete(cmd.ActorId); // still throws if IsSystemRole — unchanged behavior
            _roles.Update(role);
            await _roles.SaveChangesAsync(ct);

            await _audit.WriteAsync(
                tableName: "roles",
                recordId: role.Id,
                changeType: "AccessLevelDeleted",
                actorId: cmd.ActorId,
                oldValues: $"{{\"name\":\"{role.Name}\"}}",
                newValues: null,
                ct: ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}



public sealed class DeactivateRoleCommandHandler : IRequestHandler<DeactivateRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    private readonly IAuditLogWriter _audit;

    public DeactivateRoleCommandHandler(IRoleRepository roles, IUserRoleRepository userRoles, IAuditLogWriter audit)
        => (_roles, _userRoles, _audit) = (roles, userRoles, audit);

    public async Task<Result> Handle(DeactivateRoleCommand cmd, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(cmd.RoleId, ct);
        if (role is null) return Result.Failure($"Role {cmd.RoleId} not found.");

        // FR-RP-024 — block if ANY active agent currently holds it. Note: "active agent"
        // here means the AGENT's own active status. CountByRoleAsync as written in PATCH 29
        // counts all non-deleted UserRole rows regardless of the agent's own active flag —
        // if you need the stricter "active agent" reading, CountByRoleAsync needs a join to
        // Users and a WHERE u.is_active = true clause. Flagging this distinction explicitly
        // since the FSD wording differs between FR-RP-024 (active agents) and BR-RP-005
        // (active or inactive agents) — they are NOT the same check.
        var holderCount = await _userRoles.CountByRoleAsync(cmd.RoleId, ct);
        if (holderCount > 0)
            return Result.Failure(
                $"Cannot deactivate: {holderCount} agent(s) currently hold this Access Level.");

        role.DeactivateAccessLevel(cmd.ActorId); // from PATCH 26
        _roles.Update(role);
        await _roles.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            tableName: "roles",
            recordId: role.Id,
            changeType: "AccessLevelDeactivated",
            actorId: cmd.ActorId,
            oldValues: "{\"isActive\":true}",
            newValues: "{\"isActive\":false}",
            ct: ct);

        return Result.Success();
    }
}


public sealed class CloneRoleCommandHandler : IRequestHandler<CloneRoleCommand, Result<Guid>>
{
    private readonly IRoleRepository _roles;
    private readonly IRolePermissionRepository _rp;
    private readonly IAuditLogWriter _audit;

    public CloneRoleCommandHandler(IRoleRepository roles, IRolePermissionRepository rp, IAuditLogWriter audit)
        => (_roles, _rp, _audit) = (roles, rp, audit);

    public async Task<Result<Guid>> Handle(CloneRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var source = await _roles.GetByIdAsync(cmd.SourceRoleId, ct);
            if (source is null) return Result<Guid>.Failure($"Source Access Level {cmd.SourceRoleId} not found.");

            if (await _roles.ExistsByNameAsync(cmd.NewRoleName, ct))
                return Result<Guid>.Failure($"An Access Level named '{cmd.NewRoleName}' already exists.");

            // New Access Level is never a System Default, regardless of the source —
            // cloning is explicitly a user action, not a platform-shipped default.
            var clone = Role.Create(cmd.NewRoleName, cmd.NewRoleDescription, cmd.ActorId);
            _roles.Add(clone);
            await _roles.SaveChangesAsync(ct); // need clone.Id persisted before adding RolePermission rows referencing it

            var sourcePermissions = await _rp.GetByRoleWithPermissionsAsync(cmd.SourceRoleId, ct);
            foreach (var sourceRp in sourcePermissions)
                _rp.Add(RolePermission.Assign(clone.Id, sourceRp.PermissionId, cmd.ActorId));

            await _rp.SaveChangesAsync(ct);

            await _audit.WriteAsync(
                tableName: "roles",
                recordId: clone.Id,
                changeType: "AccessLevelCloned",
                actorId: cmd.ActorId,
                oldValues: $"{{\"sourceRoleId\":\"{cmd.SourceRoleId}\",\"sourceName\":\"{source.Name}\"}}",
                newValues: $"{{\"name\":\"{clone.Name}\",\"copiedPermissionCount\":{sourcePermissions.Count}}}",
                ct: ct);

            return Result<Guid>.Success(clone.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
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


public sealed class CopyPermissionsFromRoleCommandHandler : IRequestHandler<CopyPermissionsFromRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IRolePermissionRepository _rp;
    private readonly IAuditLogWriter _audit;

    public CopyPermissionsFromRoleCommandHandler(IRoleRepository roles, IRolePermissionRepository rp, IAuditLogWriter audit)
        => (_roles, _rp, _audit) = (roles, rp, audit);

    public async Task<Result> Handle(CopyPermissionsFromRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            if (cmd.SourceRoleId == cmd.TargetRoleId)
                return Result.Failure("Source and target Access Level cannot be the same.");

            var source = await _roles.GetByIdAsync(cmd.SourceRoleId, ct);
            if (source is null) return Result.Failure($"Source Access Level {cmd.SourceRoleId} not found.");

            var target = await _roles.GetByIdAsync(cmd.TargetRoleId, ct);
            if (target is null) return Result.Failure($"Target Access Level {cmd.TargetRoleId} not found.");

            var sourcePermissions = await _rp.GetByRoleWithPermissionsAsync(cmd.SourceRoleId, ct);
            var targetPermissionsBefore = await _rp.GetByRoleWithPermissionsAsync(cmd.TargetRoleId, ct);

            // BR-RP-001 — would this overwrite strip the target's Role & Permission Mgmt
            // edit access, and is the target the last Active role holding it?
            var targetHadLockoutPerm = targetPermissionsBefore.Any(x =>
                x.Permission.Resource == LockoutGuardConstants.LOCKOUT_RESOURCE &&
                x.Permission.Action == LockoutGuardConstants.LOCKOUT_ACTION);
            var sourceHasLockoutPerm = sourcePermissions.Any(x =>
                x.Permission.Resource == LockoutGuardConstants.LOCKOUT_RESOURCE &&
                x.Permission.Action == LockoutGuardConstants.LOCKOUT_ACTION);

            if (targetHadLockoutPerm && !sourceHasLockoutPerm)
            {
                var remainingOtherHolders = await _rp.CountActiveRolesWithPermissionAsync(
                    LockoutGuardConstants.LOCKOUT_RESOURCE, LockoutGuardConstants.LOCKOUT_ACTION, cmd.TargetRoleId, ct);

                if (remainingOtherHolders == 0)
                    return Result.Failure(
                        "Cannot copy: this would remove the last Active Access Level's Edit access to " +
                        "Role & Permission Management, causing a platform-wide configuration lockout.");
            }

            // FR-RP-033 — full overwrite of target's existing permissions.
            await _rp.SoftDeleteByRoleAsync(cmd.TargetRoleId, cmd.ActorId, ct);
            foreach (var sourceRp in sourcePermissions)
                _rp.Add(RolePermission.Assign(cmd.TargetRoleId, sourceRp.PermissionId, cmd.ActorId));
            await _rp.SaveChangesAsync(ct);

            await _audit.WriteAsync(
                tableName: "role_permissions",
                recordId: cmd.TargetRoleId,
                changeType: "PermissionMatrixCopiedFrom",
                actorId: cmd.ActorId,
                oldValues: $"{{\"permissionCount\":{targetPermissionsBefore.Count}}}",
                newValues: $"{{\"copiedFromRoleId\":\"{cmd.SourceRoleId}\",\"copiedFromName\":\"{source.Name}\",\"permissionCount\":{sourcePermissions.Count}}}",
                ct: ct);

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
    private readonly IAuditLogWriter _audit;

    public RevokePermissionFromRoleCommandHandler(IRolePermissionRepository rp, IAuditLogWriter audit)
        => (_rp, _audit) = (rp, audit);

    public async Task<Result> Handle(RevokePermissionFromRoleCommand cmd, CancellationToken ct)
    {
        try
        {
            var rp = await _rp.GetAsync(cmd.RoleId, cmd.PermissionId, ct);
            if (rp is null) return Result.Failure("Permission is not assigned to this role.");

            // BR-RP-001 lockout guard — only relevant if the permission being revoked IS
            // the lockout permission itself.
            if (rp.Permission is not null
                && rp.Permission.Resource == LockoutGuardConstants.LOCKOUT_RESOURCE
                && rp.Permission.Action == LockoutGuardConstants.LOCKOUT_ACTION)
            {
                var remainingOtherHolders = await _rp.CountActiveRolesWithPermissionAsync(
                    LockoutGuardConstants.LOCKOUT_RESOURCE, LockoutGuardConstants.LOCKOUT_ACTION, cmd.RoleId, ct);

                if (remainingOtherHolders == 0)
                    return Result.Failure(
                        "Cannot remove this permission: at least one Active Access Level must always " +
                        "retain Edit access to Role & Permission Management, to prevent a platform-wide " +
                        "configuration lockout.");
            }

            rp.SoftDelete(cmd.ActorId);
            _rp.Update(rp);
            await _rp.SaveChangesAsync(ct);

            await _audit.WriteAsync(
                tableName: "role_permissions",
                recordId: cmd.RoleId,
                changeType: "PermissionToggled",
                actorId: cmd.ActorId,
                oldValues: $"{{\"permissionId\":\"{cmd.PermissionId}\",\"allowed\":true}}",
                newValues: $"{{\"permissionId\":\"{cmd.PermissionId}\",\"allowed\":false}}",
                ct: ct);

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
    private readonly IAuditLogWriter _audit;

    public SetRolePermissionsCommandHandler(
        IRoleRepository roles, IPermissionRepository perms, IRolePermissionRepository rp, IAuditLogWriter audit)
        => (_roles, _perms, _rp, _audit) = (roles, perms, rp, audit);

    public async Task<Result> Handle(SetRolePermissionsCommand cmd, CancellationToken ct)
    {
        try
        {
            var role = await _roles.GetByIdAsync(cmd.RoleId, ct);
            if (role is null) return Result.Failure($"Role {cmd.RoleId} not found.");

            var permissionsToAssign = new List<Permission>();
            foreach (var permId in cmd.PermissionIds)
            {
                var perm = await _perms.GetByIdAsync(permId, ct);
                if (perm is null) return Result.Failure($"Permission {permId} not found.");
                permissionsToAssign.Add(perm);
            }

            // BR-RP-001 — is the lockout permission present in the role's CURRENT grants
            // but absent from the NEW set being applied? If so, this call would remove it.
            var currentlyHasLockoutPermission = await _rp.GetByRoleWithPermissionsAsync(cmd.RoleId, ct);
            var hadLockoutPermBefore = currentlyHasLockoutPermission.Any(x =>
                x.Permission.Resource == LockoutGuardConstants.LOCKOUT_RESOURCE &&
                x.Permission.Action == LockoutGuardConstants.LOCKOUT_ACTION);

            var willHaveLockoutPermAfter = permissionsToAssign.Any(p =>
                p.Resource == LockoutGuardConstants.LOCKOUT_RESOURCE &&
                p.Action == LockoutGuardConstants.LOCKOUT_ACTION);

            if (hadLockoutPermBefore && !willHaveLockoutPermAfter)
            {
                var remainingOtherHolders = await _rp.CountActiveRolesWithPermissionAsync(
                    LockoutGuardConstants.LOCKOUT_RESOURCE, LockoutGuardConstants.LOCKOUT_ACTION, cmd.RoleId, ct);

                if (remainingOtherHolders == 0)
                    return Result.Failure(
                        "Cannot apply this permission set: at least one Active Access Level must always " +
                        "retain Edit access to Role & Permission Management, to prevent a platform-wide " +
                        "configuration lockout.");
            }

            await _rp.SoftDeleteByRoleAsync(cmd.RoleId, cmd.ActorId, ct);
            foreach (var permId in cmd.PermissionIds)
                _rp.Add(RolePermission.Assign(cmd.RoleId, permId, cmd.ActorId));
            await _rp.SaveChangesAsync(ct);

            await _audit.WriteAsync(
                tableName: "role_permissions",
                recordId: cmd.RoleId,
                changeType: "PermissionMatrixReplaced",
                actorId: cmd.ActorId,
                oldValues: $"{{\"permissionIds\":[{string.Join(",", currentlyHasLockoutPermission.Select(x => $"\"{x.PermissionId}\""))}]}}",
                newValues: $"{{\"permissionIds\":[{string.Join(",", cmd.PermissionIds.Select(id => $"\"{id}\""))}]}}",
                ct: ct);

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

public sealed class SetUserAccessLevelCommandHandler : IRequestHandler<SetUserAccessLevelCommand, Result>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _ur;
    private readonly IAuditLogWriter _audit;

    public SetUserAccessLevelCommandHandler(
        IUserRepository users, IRoleRepository roles, IUserRoleRepository ur, IAuditLogWriter audit)
        => (_users, _roles, _ur, _audit) = (users, roles, ur, audit);

    public async Task<Result> Handle(SetUserAccessLevelCommand cmd, CancellationToken ct)
    {
        if (await _users.GetByIdAsync(cmd.UserId, ct) is null)
            return Result.Failure($"User {cmd.UserId} not found.");

        var newAccessLevel = await _roles.GetByIdAsync(cmd.AccessLevelId, ct);
        if (newAccessLevel is null)
            return Result.Failure($"Access Level {cmd.AccessLevelId} not found.");

        // FR-RP-040 — server-side re-check; don't trust the client sent an active ID.
        // (Role entity needs an IsActive concept for this to be meaningful — see the
        // separate Role/ActiveSoftDeleteEntity patch note below if Role doesn't already
        // derive from ActiveSoftDeleteEntity. Based on the source I reviewed, Role
        // currently derives from SoftDeleteEntity, NOT ActiveSoftDeleteEntity — meaning
        // there is no IsActive/Status field on Access Level at all yet. This check is
        // written against the field FS-05 requires; you'll need to add IsActive to Role
        // first (see PATCH note 26) before this line compiles.)
        if (!newAccessLevel.IsActive)
            return Result.Failure($"Access Level '{newAccessLevel.Name}' is inactive and cannot be newly assigned.");

        var existing = await _ur.GetByUserAsync(cmd.UserId, ct);
        var oldAccessLevel = existing.FirstOrDefault();
        var oldName = oldAccessLevel?.Role?.Name ?? "(none)";

        if (oldAccessLevel is not null && oldAccessLevel.RoleId == cmd.AccessLevelId)
            return Result.Success(); // no-op, already holds this Access Level

        // FR-RP-041 — replace, never append. This is what makes "exactly one" hold:
        // every existing UserRole row for this user is removed before the new one is added.
        await _ur.SoftDeleteByUserAsync(cmd.UserId, cmd.ActorId, ct);
        _ur.Add(UserRole.Assign(cmd.UserId, cmd.AccessLevelId, cmd.ActorId));
        await _ur.SaveChangesAsync(ct);

        // FR-RP-045 — audit old/new Access Level, actor, timestamp.
        await _audit.WriteAsync(
            tableName: "user_roles",
            recordId: cmd.UserId,
            changeType: "AgentAccessLevelChanged",
            actorId: cmd.ActorId,
            oldValues: $"{{\"accessLevel\":\"{oldName}\"}}",
            newValues: $"{{\"accessLevel\":\"{newAccessLevel.Name}\"}}",
            ct: ct);

        return Result.Success();
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

            if (cmd.FallbackGroupId.HasValue)
            {
                var fallbackService = new GroupFallbackValidationService(_groups);
                // On create, sourceGroupId is new, so we use Guid.NewGuid() temporarily or just the target
                var validationResult = await fallbackService.ValidateFallbackChainAsync(Guid.Empty, cmd.FallbackGroupId.Value, ct);
                if (!validationResult.IsSuccess) return Result<Guid>.Failure(validationResult.Error ?? "Fallback validation failed.");
            }

            var group = Group.Create(cmd.Name, cmd.RegionCode, cmd.TierCode, cmd.UnattendedAlertMinutes, cmd.ActorId, cmd.AssignmentStrategy, cmd.FallbackGroupId);
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

            if (cmd.FallbackGroupId.HasValue)
            {
                var fallbackService = new GroupFallbackValidationService(_groups);
                var validationResult = await fallbackService.ValidateFallbackChainAsync(cmd.GroupId, cmd.FallbackGroupId.Value, ct);
                if (!validationResult.IsSuccess) return validationResult;
            }

            group.Update(cmd.Name, cmd.RegionCode, cmd.TierCode, cmd.UnattendedAlertMinutes, cmd.ActorId, cmd.AssignmentStrategy, cmd.FallbackGroupId);
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
    private readonly IEventBus _eventBus;
    public RemoveUserFromGroupCommandHandler(IUserGroupRepository ug, IEventBus eventBus) => (_ug, _eventBus) = (ug, eventBus);

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
    private readonly IEventBus _eventBus;
    public SetGroupLeadCommandHandler(IUserGroupRepository ug, IEventBus eventBus) => (_ug, _eventBus) = (ug, eventBus);

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
