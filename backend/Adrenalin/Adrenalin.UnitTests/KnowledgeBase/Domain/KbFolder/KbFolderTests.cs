using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Events;
using Adrenalin.Modules.KB.Domain.ValueObjects;

namespace Adrenalin.UnitTests.KnowledgeBase.Domain.KbFolderTests;

public sealed class KbFolderTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static KbFolder Make(string name = "Folder", Guid? parentId = null,
        int displayOrder = 0, int parentDepth = 0)
        => KbFolder.Create(FolderName.Create(name), parentId, displayOrder,
            createdBy: Guid.NewGuid(), currentParentDepth: parentDepth);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArgs_SetsAllProperties()
    {
        var parentId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var folder = KbFolder.Create(FolderName.Create("Support"), parentId, 3, actorId);

        Assert.Equal("Support", folder.Name);
        Assert.Equal(parentId, folder.ParentId);
        Assert.Equal(3, folder.DisplayOrder);
        Assert.Equal(actorId, folder.CreatedBy);
        Assert.False(folder.IsDeleted);
        Assert.NotEqual(Guid.Empty, folder.Id);
    }

    [Fact]
    public void Create_WithoutParent_ParentIdIsNull()
    {
        var folder = Make();
        Assert.Null(folder.ParentId);
    }

    [Fact]
    public void Create_RaisesKbFolderCreatedDomainEvent()
    {
        var folder = Make();
        Assert.Single(folder.DomainEvents);
        Assert.IsType<KbFolderCreatedDomainEvent>(folder.DomainEvents[0]);
    }

    [Fact]
    public void Create_AtMaxDepth_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Make(parentId: Guid.NewGuid(), parentDepth: KbFolder.MaxDepth));

        Assert.Contains(KbFolder.MaxDepth.ToString(), ex.Message);
    }

    [Fact]
    public void Create_OneLevelBelowMaxDepth_Succeeds()
    {
        var folder = Make(parentId: Guid.NewGuid(), parentDepth: KbFolder.MaxDepth - 1);
        Assert.NotNull(folder);
    }

    [Fact]
    public void Create_WithNoParent_DepthCheckNotApplied()
    {
        // parentId = null → depth guard is skipped regardless of depth value
        var folder = KbFolder.Create(FolderName.Create("Root"), null, 0,
            Guid.NewGuid(), currentParentDepth: 999);
        Assert.NotNull(folder);
    }

    // ── Rename ────────────────────────────────────────────────────────────────

    [Fact]
    public void Rename_SetsNameAndAuditFields()
    {
        var actorId = Guid.NewGuid();
        var folder = Make();

        folder.Rename(FolderName.Create("NewName"), actorId);

        Assert.Equal("NewName", folder.Name);
        Assert.Equal(actorId, folder.UpdatedBy);
        Assert.NotNull(folder.UpdatedAt);
    }

    [Fact]
    public void Rename_DeletedFolder_ThrowsInvalidOperationException()
    {
        var actor = Guid.NewGuid();
        var folder = Make();
        folder.SoftDelete(actor);

        Assert.Throws<InvalidOperationException>(() =>
            folder.Rename(FolderName.Create("X"), actor));
    }

    // ── Reorder ───────────────────────────────────────────────────────────────

    [Fact]
    public void Reorder_UpdatesDisplayOrderAndAudit()
    {
        var actorId = Guid.NewGuid();
        var folder = Make();

        folder.Reorder(7, actorId);

        Assert.Equal(7, folder.DisplayOrder);
        Assert.Equal(actorId, folder.UpdatedBy);
        Assert.NotNull(folder.UpdatedAt);
    }

    // ── SoftDelete ────────────────────────────────────────────────────────────

    [Fact]
    public void SoftDelete_SetsIsDeletedAndRaisesEvent()
    {
        var actorId = Guid.NewGuid();
        var folder = Make();
        folder.ClearDomainEvents();

        folder.SoftDelete(actorId);

        Assert.True(folder.IsDeleted);
        Assert.Equal(actorId, folder.UpdatedBy);
        Assert.Single(folder.DomainEvents);
        Assert.IsType<KbFolderDeletedDomainEvent>(folder.DomainEvents[0]);
    }

    [Fact]
    public void SoftDelete_AlreadyDeleted_ThrowsInvalidOperationException()
    {
        var actor = Guid.NewGuid();
        var folder = Make();
        folder.SoftDelete(actor);

        Assert.Throws<InvalidOperationException>(() => folder.SoftDelete(actor));
    }

    // ── ClearDomainEvents ────────────────────────────────────────────────────

    [Fact]
    public void ClearDomainEvents_EmptiesTheList()
    {
        var folder = Make();
        Assert.NotEmpty(folder.DomainEvents);

        folder.ClearDomainEvents();

        Assert.Empty(folder.DomainEvents);
    }
}