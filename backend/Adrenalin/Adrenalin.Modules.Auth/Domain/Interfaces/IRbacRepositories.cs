using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    void Add(Role role);
    void Update(Role role);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(string resource, string action, CancellationToken ct = default);
    void Add(Permission permission);
    void Update(Permission permission);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IRolePermissionRepository
{
    Task<RolePermission?> GetAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task<IReadOnlyList<RolePermission>> GetByRoleWithPermissionsAsync(Guid roleId, CancellationToken ct = default);
    void Add(RolePermission rp);
    void Update(RolePermission rp);
    Task SoftDeleteByRoleAsync(Guid roleId, Guid actorId, CancellationToken ct = default);
    Task SoftDeleteByPermissionAsync(Guid permissionId, Guid actorId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserRoleRepository
{
    Task<UserRole?> GetAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<UserRole?> GetIncludingDeletedAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<UserRole>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    void Add(UserRole userRole);
    void Update(UserRole userRole);
    Task SoftDeleteByUserAsync(Guid userId, Guid actorId, CancellationToken ct = default);
    Task SoftDeleteByRoleAsync(Guid roleId, Guid actorId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Group?> GetWithMembersAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    void Add(Group group);
    void Update(Group group);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserGroupRepository
{
    Task<UserGroup?> GetAsync(Guid userId, Guid groupId, CancellationToken ct = default);
    Task<UserGroup?> GetIncludingDeletedAsync(Guid userId, Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<UserGroup>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserGroup>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);
    void Add(UserGroup ug);
    void Update(UserGroup ug);
    Task SoftDeleteByGroupAsync(Guid groupId, Guid actorId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
