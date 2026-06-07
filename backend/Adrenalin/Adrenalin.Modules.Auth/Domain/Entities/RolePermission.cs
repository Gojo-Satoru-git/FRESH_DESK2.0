using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class RolePermission : SoftDeleteEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
    public User? CreatedByNavigation { get; private set; }
    public User? UpdatedByNavigation { get; private set; }

    private RolePermission() { }

    public static RolePermission Assign(Guid roleId, Guid permissionId, Guid assignedBy)
    {
        if (roleId == Guid.Empty) throw new ArgumentException("roleId must not be empty.");
        if (permissionId == Guid.Empty) throw new ArgumentException("permissionId must not be empty.");
        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            IsDeleted = false,
            CreatedBy = assignedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Already removed.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}