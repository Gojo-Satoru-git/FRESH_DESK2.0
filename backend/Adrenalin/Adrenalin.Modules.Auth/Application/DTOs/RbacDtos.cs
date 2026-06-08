namespace Adrenalin.Modules.Auth.Application.DTOs;

public sealed record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record RoleSummaryDto(Guid Id, string Name, bool IsSystemRole);

public sealed record PermissionDto(Guid Id, string Resource, string Action, string? Description);

public sealed record RoleWithPermissionsDto(Guid Id, string Name, string? Description,
    bool IsSystemRole, IReadOnlyList<PermissionDto> Permissions,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record UserSummaryDto(Guid Id, string Email, string? FirstName,
    string? LastName, bool IsActive, string? Phone);

public sealed record UserWithRolesDto(Guid Id, string Email, string? FirstName,
    string? LastName, bool IsActive, IReadOnlyList<RoleSummaryDto> Roles,
    DateTimeOffset CreatedAt, DateTimeOffset? LastLoginAt);

public sealed record GroupDto(Guid Id, string Name, string? RegionCode, string? TierCode,
    int UnattendedAlertMinutes, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record GroupMemberDto(Guid UserId, string Email, string? FirstName,
    string? LastName, bool IsLead);

public sealed record GroupWithMembersDto(GroupDto Group, IReadOnlyList<GroupMemberDto> Members);

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount,
    int PageNumber, int PageSize);
