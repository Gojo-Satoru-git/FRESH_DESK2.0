using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class Role : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Role() { }

    public static Role Create(string name, string? description, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.");
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsSystemRole = false,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string? description, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted role.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.");
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsSystemRole) throw new InvalidOperationException("System roles cannot be deleted.");
        if (IsDeleted) throw new InvalidOperationException("Role is already deleted.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>FR-RP-024 — entity-level state change only. Caller (handler) must verify
    /// zero active agents hold this Access Level BEFORE calling this method.</summary>
    public void DeactivateAccessLevel(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot deactivate a deleted Access Level.");
        Deactivate();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>FR-RP-005-equivalent for Access Level — restores visibility in pickers.</summary>
    public void ReactivateAccessLevel(Guid actorId)
    {
        Activate();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}