using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Commands;

public static class LockoutGuardConstants
{
    public const string LOCKOUT_RESOURCE = "role"; // CONFIRM against real seed data
    public const string LOCKOUT_ACTION = "write";  // CONFIRM against real seed data
}

// ── Roles ─────────────────────────────────────────────────────────────────────
public sealed record CreateRoleCommand(string Name, string? Description, Guid ActorId)
    : IRequest<Result<Guid>>;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, string? Description, Guid ActorId)
    : IRequest<Result>;

public sealed record DeleteRoleCommand(Guid RoleId, Guid ActorId)
    : IRequest<Result>;
public sealed record DeactivateRoleCommand(
    Guid RoleId,
    Guid ActorId
) : IRequest<Result>;

public sealed record CloneRoleCommand(
    Guid SourceRoleId,
    string NewRoleName,
    string? NewRoleDescription,
    Guid ActorId
) : IRequest<Result<Guid>>;


// ── Permissions ───────────────────────────────────────────────────────────────
public sealed record CreatePermissionCommand(string Resource, string Action,
    string? Description, Guid ActorId) : IRequest<Result<Guid>>;

public sealed record DeletePermissionCommand(Guid PermissionId, Guid ActorId)
    : IRequest<Result>;

public sealed record CopyPermissionsFromRoleCommand(
    Guid SourceRoleId,
    Guid TargetRoleId,
    Guid ActorId
) : IRequest<Result>;

// ── Role ↔ Permission ─────────────────────────────────────────────────────────
public sealed record GrantPermissionToRoleCommand(Guid RoleId, Guid PermissionId, Guid ActorId)
    : IRequest<Result>;

public sealed record RevokePermissionFromRoleCommand(Guid RoleId, Guid PermissionId, Guid ActorId)
    : IRequest<Result>;

public sealed record SetRolePermissionsCommand(Guid RoleId,
    IReadOnlyList<Guid> PermissionIds, Guid ActorId) : IRequest<Result>;

// ── User ↔ Role ───────────────────────────────────────────────────────────────
public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId, Guid ActorId)
    : IRequest<Result>;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId, Guid ActorId)
    : IRequest<Result>;

public sealed record SetUserAccessLevelCommand(
        Guid UserId,
        Guid AccessLevelId, // formerly "RoleId" — renamed to match FS-05 terminology
        Guid ActorId
    ) : IRequest<Result>;

// ── Groups ────────────────────────────────────────────────────────────────────
public sealed record CreateGroupCommand(string Name, string? RegionCode, string? TierCode,
    int UnattendedAlertMinutes, int AssignmentStrategy, Guid? FallbackGroupId, Guid ActorId) : IRequest<Result<Guid>>;

public sealed record UpdateGroupCommand(Guid GroupId, string Name, string? RegionCode,
    string? TierCode, int UnattendedAlertMinutes, int AssignmentStrategy, Guid? FallbackGroupId, Guid ActorId) : IRequest<Result>;

public sealed record DeleteGroupCommand(Guid GroupId, Guid ActorId)
    : IRequest<Result>;

// ── User ↔ Group ──────────────────────────────────────────────────────────────
public sealed record AddUserToGroupCommand(Guid UserId, Guid GroupId, bool IsLead, Guid ActorId)
    : IRequest<Result>;

public sealed record RemoveUserFromGroupCommand(Guid UserId, Guid GroupId, Guid ActorId)
    : IRequest<Result>;

public sealed record SetGroupLeadCommand(Guid UserId, Guid GroupId, bool IsLead, Guid ActorId)
    : IRequest<Result>;
