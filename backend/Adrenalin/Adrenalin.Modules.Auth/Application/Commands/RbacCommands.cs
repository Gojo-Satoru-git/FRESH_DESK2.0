using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Commands;

// ── Roles ─────────────────────────────────────────────────────────────────────
public sealed record CreateRoleCommand(string Name, string? Description, Guid ActorId)
    : IRequest<Result<Guid>>;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, string? Description, Guid ActorId)
    : IRequest<Result>;

public sealed record DeleteRoleCommand(Guid RoleId, Guid ActorId)
    : IRequest<Result>;

// ── Permissions ───────────────────────────────────────────────────────────────
public sealed record CreatePermissionCommand(string Resource, string Action,
    string? Description, Guid ActorId) : IRequest<Result<Guid>>;

public sealed record DeletePermissionCommand(Guid PermissionId, Guid ActorId)
    : IRequest<Result>;

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

public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyList<Guid> RoleIds, Guid ActorId)
    : IRequest<Result>;

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
