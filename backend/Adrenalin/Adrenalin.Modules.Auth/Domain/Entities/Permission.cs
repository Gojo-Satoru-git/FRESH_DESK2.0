using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class Permission : SoftDeleteEntity
{
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission() { }

    public static Permission Create(string resource, string action, string? description, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(resource)) throw new ArgumentException("Resource is required.");
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.");
        return new Permission
        {
            Id = Guid.NewGuid(),
            Resource = resource.Trim().ToLowerInvariant(),
            Action = action.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public string ToKey() => $"{Resource}:{Action}";

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Permission is already deleted.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}