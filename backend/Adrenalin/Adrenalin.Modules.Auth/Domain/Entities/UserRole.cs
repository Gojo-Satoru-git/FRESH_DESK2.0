using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class UserRole : SoftDeleteEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public static UserRole Assign(Guid userId, Guid roleId, Guid assignedBy)
    {
        if (userId == Guid.Empty) throw new ArgumentException("userId required.");
        if (roleId == Guid.Empty) throw new ArgumentException("roleId required.");
        var now = DateTimeOffset.UtcNow;
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = now,
            AssignedBy = assignedBy,
            IsDeleted = false,
            CreatedBy = assignedBy,
            CreatedAt = now
        };
    }

    public void Restore(Guid assignedBy)
    {
        IsDeleted = false;
        AssignedAt = DateTimeOffset.UtcNow;
        AssignedBy = assignedBy;
        UpdatedBy = assignedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("UserRole already removed.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}