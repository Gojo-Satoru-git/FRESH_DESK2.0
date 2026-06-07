using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class Group : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? RegionCode { get; private set; }
    public string? TierCode { get; private set; }
    public int UnattendedAlertMinutes { get; private set; }

    public ICollection<UserGroup> UserGroups { get; private set; } = [];

    private Group() { }

    public static Group Create(string name, string? regionCode, string? tierCode,
        int unattendedAlertMinutes, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name required.");
        if (unattendedAlertMinutes < 1) throw new ArgumentException("UnattendedAlertMinutes must be >= 1.");
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            RegionCode = regionCode?.Trim().ToUpperInvariant(),
            TierCode = tierCode?.Trim().ToUpperInvariant(),
            UnattendedAlertMinutes = unattendedAlertMinutes,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string? regionCode, string? tierCode,
        int unattendedAlertMinutes, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted group.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name required.");
        if (unattendedAlertMinutes < 1) throw new ArgumentException("UnattendedAlertMinutes must be >= 1.");
        Name = name.Trim();
        RegionCode = regionCode?.Trim().ToUpperInvariant();
        TierCode = tierCode?.Trim().ToUpperInvariant();
        UnattendedAlertMinutes = unattendedAlertMinutes;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Group already deleted.");
        IsDeleted = true;
        IsActive = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}