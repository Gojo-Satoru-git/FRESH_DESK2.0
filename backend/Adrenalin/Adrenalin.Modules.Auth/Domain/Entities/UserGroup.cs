using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class UserGroup : SoftDeleteEntity
{
    public Guid UserId { get; private set; }
    public Guid GroupId { get; private set; }
    public bool IsLead { get; private set; }


    public User User { get; private set; } = null!;
    public Group Group { get; private set; } = null!;

    private UserGroup() { }

    public static UserGroup Add(Guid userId, Guid groupId, bool isLead, Guid addedBy)
    {
        if (userId == Guid.Empty) throw new ArgumentException("userId required.");
        if (groupId == Guid.Empty) throw new ArgumentException("groupId required.");
        return new UserGroup
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            IsLead = isLead,

            IsDeleted = false,
            CreatedBy = addedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Restore(bool isLead, Guid updatedBy)
    {
        IsDeleted = false;
        IsLead = isLead;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetLead(bool isLead, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify removed membership.");
        IsLead = isLead;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Membership already removed.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}