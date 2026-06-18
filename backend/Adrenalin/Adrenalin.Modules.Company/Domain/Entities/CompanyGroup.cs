using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

/// <summary>
/// Maps a Company to a support Group (many-to-many).
/// A company may be supported by multiple groups.
/// One mapping per company may be flagged as default (fallback for routing).
/// </summary>
public sealed class CompanyGroup : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }
    public Guid GroupId { get; private set; }

    /// <summary>
    /// When true, this group is the fallback/default for the company
    /// if no routing rule matches. Only one per company should be default.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Ordering hint — lower values are higher priority.
    /// Used when displaying or selecting among multiple groups.
    /// </summary>
    public int Priority { get; private set; }

    private CompanyGroup() { }

    public static CompanyGroup Create(Guid companyId, Guid groupId, bool isDefault, int priority, Guid createdBy)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId is required.");
        if (groupId == Guid.Empty) throw new ArgumentException("GroupId is required.");
        if (priority < 0) throw new ArgumentException("Priority must be >= 0.");

        return new CompanyGroup
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            GroupId = groupId,
            IsDefault = isDefault,
            Priority = priority,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetDefault(bool isDefault, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted mapping.");
        IsDefault = isDefault;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePriority(int priority, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted mapping.");
        if (priority < 0) throw new ArgumentException("Priority must be >= 0.");
        Priority = priority;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Restore(bool isDefault, int priority, Guid updatedBy)
    {
        IsDeleted = false;
        IsDefault = isDefault;
        Priority = priority;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Mapping already deleted.");
        IsDeleted = true;
        IsDefault = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
