using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.Handlers;
using Adrenalin.Modules.KB.Application.Services;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using NSubstitute;

namespace Adrenalin.UnitTests.KnowledgeBase.Application.Handlers;

// ─── Shared helpers ───────────────────────────────────────────────────────────

file static class Builders
{
    public static KbArticle Draft(Guid? folderId = null)
        => KbArticle.Create(ArticleTitle.Create("Article"), "Body", ArticleType.Faq,
            Guid.NewGuid(), folderId, Guid.NewGuid());

    public static KbArticle Published()
    {
        var a = Draft();
        a.Publish(Guid.NewGuid());
        a.ClearDomainEvents();
        return a;
    }

    public static KbFolder Folder(bool isDeleted = false)
    {
        var f = KbFolder.Create(FolderName.Create("F"), null, 0, Guid.NewGuid());
        if (isDeleted) f.SoftDelete(Guid.NewGuid());
        return f;
    }

    public static PortalBanner Banner(bool isActive = true)
    {
        var b = PortalBanner.Create("T", "M", null, null, Guid.NewGuid());
        if (!isActive) b.Deactivate(Guid.NewGuid());
        return b;
    }
}

// ─── KbFolder Handlers ────────────────────────────────────────────────────────

public class KbFolderCommandHandlerTests
{
    private readonly IKbFolderRepository _repo = Substitute.For<IKbFolderRepository>();

    public sealed class CreateTests : KbFolderCommandHandlerTests
    {
        private readonly CreateKbFolderCommandHandler _h;
        public CreateTests() => _h = new(_repo);

        [Fact]
        public async Task Handle_NoParent_CreatesAndReturnsId()
        {
            _repo.SaveChangesAsync(default).ReturnsForAnyArgs(1);
            var result = await _h.Handle(new CreateKbFolderCommand("Root", null, 0, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            _repo.Received(1).Add(Arg.Any<KbFolder>());
        }

        [Fact]
        public async Task Handle_ValidParent_QueriesDepthAndSucceeds()
        {
            var parentId = Guid.NewGuid();
            _repo.GetByIdAsync(parentId, default).ReturnsForAnyArgs(Builders.Folder());
            _repo.GetDepthAsync(parentId, default).ReturnsForAnyArgs(2);
            _repo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new CreateKbFolderCommand("Child", parentId, 0, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            await _repo.Received(1).GetDepthAsync(parentId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ParentNotFound_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbFolder?)null);
            var result = await _h.Handle(new CreateKbFolderCommand("X", Guid.NewGuid(), 0, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error);
        }

        [Fact]
        public async Task Handle_ParentIsDeleted_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs(Builders.Folder(isDeleted: true));
            var result = await _h.Handle(new CreateKbFolderCommand("X", Guid.NewGuid(), 0, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("deleted", result.Error);
        }

        [Fact]
        public async Task Handle_ExceedsMaxDepth_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs(Builders.Folder());
            _repo.GetDepthAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs(KbFolder.MaxDepth);

            var result = await _h.Handle(new CreateKbFolderCommand("Deep", Guid.NewGuid(), 0, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class RenameTests : KbFolderCommandHandlerTests
    {
        private readonly RenameKbFolderCommandHandler _h;
        public RenameTests() => _h = new(_repo);

        [Fact]
        public async Task Handle_FolderExists_RenamesSuccessfully()
        {
            var id = Guid.NewGuid();
            var folder = Builders.Folder();
            _repo.GetByIdAsync(id, default).ReturnsForAnyArgs(folder);
            _repo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new RenameKbFolderCommand(id, "New", Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal("New", folder.Name);
            _repo.Received(1).Update(folder);
        }

        [Fact]
        public async Task Handle_FolderNotFound_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbFolder?)null);
            var result = await _h.Handle(new RenameKbFolderCommand(Guid.NewGuid(), "X", Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error);
        }

        [Fact]
        public async Task Handle_DeletedFolder_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs(Builders.Folder(isDeleted: true));
            var result = await _h.Handle(new RenameKbFolderCommand(Guid.NewGuid(), "X", Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class ReorderTests : KbFolderCommandHandlerTests
    {
        private readonly ReorderKbFolderCommandHandler _h;
        public ReorderTests() => _h = new(_repo);

        [Fact]
        public async Task Handle_FolderExists_UpdatesDisplayOrder()
        {
            var id = Guid.NewGuid();
            var folder = Builders.Folder();
            _repo.GetByIdAsync(id, default).ReturnsForAnyArgs(folder);
            _repo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new ReorderKbFolderCommand(id, 5, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, folder.DisplayOrder);
        }

        [Fact]
        public async Task Handle_FolderNotFound_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbFolder?)null);
            var result = await _h.Handle(new ReorderKbFolderCommand(Guid.NewGuid(), 3, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class DeleteTests : KbFolderCommandHandlerTests
    {
        private readonly DeleteKbFolderCommandHandler _h;
        public DeleteTests() => _h = new(_repo);

        [Fact]
        public async Task Handle_EmptyFolder_SoftDeletesSuccessfully()
        {
            var id = Guid.NewGuid();
            var folder = Builders.Folder();
            _repo.GetByIdAsync(id, default).ReturnsForAnyArgs(folder);
            _repo.HasArticlesAsync(id, default).ReturnsForAnyArgs(false);
            _repo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new DeleteKbFolderCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.True(folder.IsDeleted);
        }

        [Fact]
        public async Task Handle_FolderHasArticles_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            _repo.GetByIdAsync(id, default).ReturnsForAnyArgs(Builders.Folder());
            _repo.HasArticlesAsync(id, default).ReturnsForAnyArgs(true);

            var result = await _h.Handle(new DeleteKbFolderCommand(id, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("articles", result.Error);
        }

        [Fact]
        public async Task Handle_FolderNotFound_ReturnsFailure()
        {
            _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbFolder?)null);
            var result = await _h.Handle(new DeleteKbFolderCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }
}

// ─── KbArticle Handlers ───────────────────────────────────────────────────────

public class KbArticleCommandHandlerTests
{
    private readonly IKbArticleRepository _articleRepo = Substitute.For<IKbArticleRepository>();
    private readonly IKbFolderRepository _folderRepo = Substitute.For<IKbFolderRepository>();

    public sealed class CreateTests : KbArticleCommandHandlerTests
    {
        private readonly CreateKbArticleCommandHandler _h;
        public CreateTests() => _h = new(_articleRepo, _folderRepo);

        [Fact]
        public async Task Handle_NoFolder_CreatesAndReturnsId()
        {
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);
            var result = await _h.Handle(
                new CreateKbArticleCommand("T", null, ArticleType.Faq, null, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            _articleRepo.Received(1).Add(Arg.Any<KbArticle>());
        }

        [Fact]
        public async Task Handle_ValidFolder_CreatesArticle()
        {
            var folderId = Guid.NewGuid();
            _folderRepo.GetByIdAsync(folderId, default).ReturnsForAnyArgs(Builders.Folder());
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new CreateKbArticleCommand("T", null, ArticleType.UserManual, folderId, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_FolderNotFound_ReturnsFailure()
        {
            var folderId = Guid.NewGuid();
            _folderRepo.GetByIdAsync(folderId, default).ReturnsForAnyArgs((KbFolder?)null);

            var result = await _h.Handle(
                new CreateKbArticleCommand("T", null, ArticleType.Faq, folderId, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error);
        }

        [Fact]
        public async Task Handle_DeletedFolder_ReturnsFailure()
        {
            var folderId = Guid.NewGuid();
            _folderRepo.GetByIdAsync(folderId, default).ReturnsForAnyArgs(Builders.Folder(isDeleted: true));

            var result = await _h.Handle(
                new CreateKbArticleCommand("T", null, ArticleType.Faq, folderId, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("deleted", result.Error);
        }
    }

    public sealed class UpdateTests : KbArticleCommandHandlerTests
    {
        private readonly UpdateKbArticleCommandHandler _h;
        public UpdateTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ArticleExists_UpdatesContent()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new UpdateKbArticleCommand(id, "New Title", "New Body", Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Title", article.Title);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(
                new UpdateKbArticleCommand(Guid.NewGuid(), "X", null, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ArchivedArticle_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            article.Archive(Guid.NewGuid());
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);

            var result = await _h.Handle(
                new UpdateKbArticleCommand(id, "X", null, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class MoveTests : KbArticleCommandHandlerTests
    {
        private readonly MoveKbArticleCommandHandler _h;
        public MoveTests() => _h = new(_articleRepo, _folderRepo);

        [Fact]
        public async Task Handle_ValidTarget_MovesArticle()
        {
            var articleId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(articleId, default).ReturnsForAnyArgs(article);
            _folderRepo.GetByIdAsync(targetId, default).ReturnsForAnyArgs(Builders.Folder());
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new MoveKbArticleCommand(articleId, targetId, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(targetId, article.FolderId);
        }

        [Fact]
        public async Task Handle_NullTarget_MovesToRoot()
        {
            var articleId = Guid.NewGuid();
            var article = Builders.Draft(Guid.NewGuid());
            _articleRepo.GetByIdAsync(articleId, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new MoveKbArticleCommand(articleId, null, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Null(article.FolderId);
        }

        [Fact]
        public async Task Handle_TargetFolderDeleted_ReturnsFailure()
        {
            var articleId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            _articleRepo.GetByIdAsync(articleId, default).ReturnsForAnyArgs(Builders.Draft());
            _folderRepo.GetByIdAsync(targetId, default).ReturnsForAnyArgs(Builders.Folder(isDeleted: true));

            var result = await _h.Handle(
                new MoveKbArticleCommand(articleId, targetId, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class PublishTests : KbArticleCommandHandlerTests
    {
        private readonly PublishKbArticleCommandHandler _h;
        public PublishTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_Draft_PublishesSuccessfully()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new PublishKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(ArticleStatus.Published, article.Status);
        }

        [Fact]
        public async Task Handle_AlreadyPublished_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(Builders.Published());

            var result = await _h.Handle(new PublishKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(new PublishKbArticleCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class ArchiveTests : KbArticleCommandHandlerTests
    {
        private readonly ArchiveKbArticleCommandHandler _h;
        public ArchiveTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_Draft_ArchivesSuccessfully()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new ArchiveKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(ArticleStatus.Archived, article.Status);
        }

        [Fact]
        public async Task Handle_AlreadyArchived_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            article.Archive(Guid.NewGuid());
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);

            var result = await _h.Handle(new ArchiveKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class RestoreTests : KbArticleCommandHandlerTests
    {
        private readonly RestoreKbArticleToDraftCommandHandler _h;
        public RestoreTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_Archived_RestoresToDraft()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            article.Archive(Guid.NewGuid());
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new RestoreKbArticleToDraftCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(ArticleStatus.Draft, article.Status);
        }

        [Fact]
        public async Task Handle_DraftArticle_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(Builders.Draft());

            var result = await _h.Handle(new RestoreKbArticleToDraftCommand(id, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    public sealed class DeleteTests : KbArticleCommandHandlerTests
    {
        private readonly DeleteKbArticleCommandHandler _h;
        public DeleteTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ExistingArticle_SoftDeletesAndUpdates()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new DeleteKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.True(article.IsDeleted);
            _articleRepo.Received(1).Update(article);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(new DeleteKbArticleCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_AlreadyDeleted_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            article.SoftDelete(Guid.NewGuid());
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);

            var result = await _h.Handle(new DeleteKbArticleCommand(id, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }
}

// ─── AutoResolve + Attachment + Banner Handlers ───────────────────────────────

public class KbAutoResolveAndBannerHandlerTests
{
    private readonly IKbArticleRepository _articleRepo = Substitute.For<IKbArticleRepository>();
    private readonly IPortalBannerRepository _bannerRepo = Substitute.For<IPortalBannerRepository>();
    private readonly IKbFileStorageService _fileStorage = Substitute.For<IKbFileStorageService>();

    // ── EnableAutoResolveCommandHandler ───────────────────────────────────────

    public sealed class EnableAutoResolveTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly EnableAutoResolveCommandHandler _h;
        public EnableAutoResolveTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_PublishedArticle_EnablesAutoResolve()
        {
            var id = Guid.NewGuid();
            var article = Builders.Published();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var cmd = new EnableAutoResolveCommand(id, ["login"], "Reset password.", 0.75m, Guid.NewGuid());
            var result = await _h.Handle(cmd, default);

            Assert.True(result.IsSuccess);
            Assert.True(article.AutoResolve);
        }

        [Fact]
        public async Task Handle_DraftArticle_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(Builders.Draft());

            var cmd = new EnableAutoResolveCommand(id, ["kw"], "text", 0.75m, Guid.NewGuid());
            var result = await _h.Handle(cmd, default);

            Assert.False(result.IsSuccess);
            Assert.Contains("Published", result.Error);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(
                new EnableAutoResolveCommand(Guid.NewGuid(), ["kw"], "text", 0.75m, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── DisableAutoResolveCommandHandler ──────────────────────────────────────

    public sealed class DisableAutoResolveTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly DisableAutoResolveCommandHandler _h;
        public DisableAutoResolveTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ArticleExists_DisablesAutoResolve()
        {
            var id = Guid.NewGuid();
            var article = Builders.Published();
            article.EnableAutoResolve(["kw"], "text", ConfidenceThreshold.Create(0.7m), Guid.NewGuid());
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new DisableAutoResolveCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.False(article.AutoResolve);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(new DisableAutoResolveCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── MarkArticleAsGuardrailExcludedCommandHandler ──────────────────────────

    public sealed class GuardrailExcludeTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly MarkArticleAsGuardrailExcludedCommandHandler _h;
        public GuardrailExcludeTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ArticleExists_SetsGuardrailExcluded()
        {
            var id = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new MarkArticleAsGuardrailExcludedCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.True(article.GuardrailExcluded);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(
                new MarkArticleAsGuardrailExcludedCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── RecordArticleReopenedMatchCommandHandler ──────────────────────────────

    public sealed class RecordReopenedMatchTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly RecordArticleReopenedMatchCommandHandler _h;
        public RecordReopenedMatchTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ArticleExists_IncrementsReopened()
        {
            var id = Guid.NewGuid();
            var article = Builders.Published();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new RecordArticleReopenedMatchCommand(id, 0.025m), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, article.TimesReopened);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(new RecordArticleReopenedMatchCommand(Guid.NewGuid(), 0.025m), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── RecordArticleMatchCommandHandler ──────────────────────────────────────

    public sealed class RecordMatchTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly RecordArticleMatchCommandHandler _h;
        public RecordMatchTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ArticleExists_IncrementsTimesMatched()
        {
            var id = Guid.NewGuid();
            var article = Builders.Published();
            _articleRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(new RecordArticleMatchCommand(id), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, article.TimesMatched);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(new RecordArticleMatchCommand(Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── AddAttachmentCommandHandler ───────────────────────────────────────────

    public sealed class AddAttachmentTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly AddAttachmentCommandHandler _h;
        public AddAttachmentTests() => _h = new(_articleRepo, _fileStorage);

        [Fact]
        public async Task Handle_ValidArticle_SavesFileAndReturnsAttachmentId()
        {
            var articleId = Guid.NewGuid();
            var article = Builders.Draft();
            _articleRepo.GetWithAttachmentsAsync(articleId, default).ReturnsForAnyArgs(article);
            _fileStorage.SaveAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Stream>(), default)
                        .ReturnsForAnyArgs("https://cdn/file.pdf");
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            using var stream = new MemoryStream([1, 2, 3]);
            var cmd = new AddAttachmentCommand(articleId, "file.pdf", 1024L, "application/pdf", stream);
            var result = await _h.Handle(cmd, default);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            // Handler calls AddAttachment (not Update) on the repo
            _articleRepo.Received(1).AddAttachment(Arg.Any<KbAttachment>());
            _articleRepo.DidNotReceive().Update(Arg.Any<KbArticle>());
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetWithAttachmentsAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);

            using var stream = new MemoryStream();
            var result = await _h.Handle(
                new AddAttachmentCommand(Guid.NewGuid(), "f.pdf", null, null, stream), default);

            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error);
        }
    }

    // ── RemoveAttachmentCommandHandler ────────────────────────────────────────

    public sealed class RemoveAttachmentTests : KbAutoResolveAndBannerHandlerTests
    {
        private readonly RemoveAttachmentCommandHandler _h;
        public RemoveAttachmentTests() => _h = new(_articleRepo);

        [Fact]
        public async Task Handle_ValidAttachment_SoftDeletesIt()
        {
            var articleId = Guid.NewGuid();
            var article = Builders.Draft();
            var attachment = article.AddAttachment("https://cdn/f.pdf", "f.pdf", 512L, null);
            _articleRepo.GetWithAttachmentsAsync(articleId, default).ReturnsForAnyArgs(article);
            _articleRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await _h.Handle(
                new RemoveAttachmentCommand(articleId, attachment.Id), default);

            Assert.True(result.IsSuccess);
            Assert.True(attachment.IsDeleted);
            // Should NOT call Update on the article (only SaveChanges)
            _articleRepo.DidNotReceive().Update(Arg.Any<KbArticle>());
        }

        [Fact]
        public async Task Handle_AttachmentNotFound_ReturnsFailure()
        {
            var articleId = Guid.NewGuid();
            var article = Builders.Draft(); // no attachments
            _articleRepo.GetWithAttachmentsAsync(articleId, default).ReturnsForAnyArgs(article);

            var result = await _h.Handle(
                new RemoveAttachmentCommand(articleId, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ArticleNotFound_ReturnsFailure()
        {
            _articleRepo.GetWithAttachmentsAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((KbArticle?)null);
            var result = await _h.Handle(
                new RemoveAttachmentCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }

    // ── Portal Banner Handlers ────────────────────────────────────────────────

    public sealed class PortalBannerHandlerTests : KbAutoResolveAndBannerHandlerTests
    {
        [Fact]
        public async Task CreateBanner_ValidCommand_ReturnsId()
        {
            var h = new CreatePortalBannerCommandHandler(_bannerRepo);
            _bannerRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var cmd = new CreatePortalBannerCommand("Title", "Message", null, null, Guid.NewGuid());
            var result = await h.Handle(cmd, default);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            _bannerRepo.Received(1).Add(Arg.Any<PortalBanner>());
        }

        [Fact]
        public async Task CreateBanner_InvalidSchedule_ReturnsFailure()
        {
            var h = new CreatePortalBannerCommandHandler(_bannerRepo);
            var now = DateTimeOffset.UtcNow;
            var cmd = new CreatePortalBannerCommand("T", "M", now.AddHours(5), now.AddHours(1), Guid.NewGuid());

            var result = await h.Handle(cmd, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateBanner_BannerExists_UpdatesSuccessfully()
        {
            var h = new UpdatePortalBannerCommandHandler(_bannerRepo);
            var id = Guid.NewGuid();
            var banner = Builders.Banner();
            _bannerRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(banner);
            _bannerRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await h.Handle(
                new UpdatePortalBannerCommand(id, "New Title", "New Message", null, null, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Title", banner.Title);
        }

        [Fact]
        public async Task UpdateBanner_NotFound_ReturnsFailure()
        {
            var h = new UpdatePortalBannerCommandHandler(_bannerRepo);
            _bannerRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((PortalBanner?)null);

            var result = await h.Handle(
                new UpdatePortalBannerCommand(Guid.NewGuid(), "T", "M", null, null, Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ActivateBanner_BannerExists_SetsActive()
        {
            var h = new ActivatePortalBannerCommandHandler(_bannerRepo);
            var id = Guid.NewGuid();
            var banner = Builders.Banner(isActive: false);
            _bannerRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(banner);
            _bannerRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await h.Handle(new ActivatePortalBannerCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.True(banner.IsActive);
        }

        [Fact]
        public async Task DeactivateBanner_BannerExists_SetsInactive()
        {
            var h = new DeactivatePortalBannerCommandHandler(_bannerRepo);
            var id = Guid.NewGuid();
            var banner = Builders.Banner(isActive: true);
            _bannerRepo.GetByIdAsync(id, default).ReturnsForAnyArgs(banner);
            _bannerRepo.SaveChangesAsync(default).ReturnsForAnyArgs(1);

            var result = await h.Handle(new DeactivatePortalBannerCommand(id, Guid.NewGuid()), default);

            Assert.True(result.IsSuccess);
            Assert.False(banner.IsActive);
        }

        [Fact]
        public async Task ActivateBanner_NotFound_ReturnsFailure()
        {
            var h = new ActivatePortalBannerCommandHandler(_bannerRepo);
            _bannerRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((PortalBanner?)null);

            var result = await h.Handle(new ActivatePortalBannerCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateBanner_NotFound_ReturnsFailure()
        {
            var h = new DeactivatePortalBannerCommandHandler(_bannerRepo);
            _bannerRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsForAnyArgs((PortalBanner?)null);

            var result = await h.Handle(new DeactivatePortalBannerCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            Assert.False(result.IsSuccess);
        }
    }
}