using Adrenalin.Modules.KB.Domain.Events;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.KB.Domain.Entities;

/// <summary>
/// Self-referencing folder hierarchy node. Table: kb.kb_folders
/// Inherits SoftDeleteEntity → AuditableEntity → BaseEntity
///   giving: Id, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, RowVersion, IsDeleted
/// Note: kb.kb_folders has no is_active column per schema.
/// </summary>
public sealed class KbFolder : SoftDeleteEntity
{
    public const int MaxDepth = 5;

    public string Name { get; private set; } = default!;
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; }
    public KbFolder? Parent { get; private set; }

    private readonly List<INotification> _domainEvents = [];
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private KbFolder() { }

    public static KbFolder Create(FolderName name, Guid? parentId, int displayOrder,
        Guid? createdBy, int currentParentDepth = 0)
    {
        if (parentId.HasValue && currentParentDepth >= MaxDepth)
            throw new InvalidOperationException($"Folder nesting cannot exceed {MaxDepth} levels deep.");

        var folder = new KbFolder
        {
            Id = Guid.NewGuid(),
            Name = name.Value,
            ParentId = parentId,
            DisplayOrder = displayOrder,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
        folder._domainEvents.Add(new KbFolderCreatedDomainEvent(folder.Id, folder.ParentId, folder.Name));
        return folder;
    }

    public void Rename(FolderName newName, Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot rename a deleted folder.");
        Name = newName.Value;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reorder(int newDisplayOrder, Guid updatedBy)
    {
        DisplayOrder = newDisplayOrder;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Folder is already deleted.");
        IsDeleted = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new KbFolderDeletedDomainEvent(Id));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}