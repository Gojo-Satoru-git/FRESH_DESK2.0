using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class RolePermission : SoftDeleteEntity
{
    private RolePermission() { }

    public static RolePermission Assign(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty) throw new ArgumentException("roleId must not be empty.", nameof(roleId));
        if (permissionId == Guid.Empty) throw new ArgumentException("permissionId must not be empty.", nameof(permissionId));

        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public User? CreatedByNavigation { get; private set; }

    public Permission Permission { get; private set; } = null!;

    public Role Role { get; private set; } = null!;

    public User? UpdatedByNavigation { get; private set; }
}
