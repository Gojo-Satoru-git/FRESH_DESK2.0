using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.KB.Application.Handlers;

// ─── CreateKbArticleCommandHandler ───────────────────────────────────────────

public sealed class CreateKbArticleCommandHandler
    : IRequestHandler<CreateKbArticleCommand, Result<Guid>>
{
    private readonly IKbArticleRepository _articleRepo;
    private readonly IKbFolderRepository  _folderRepo;

    public CreateKbArticleCommandHandler(
        IKbArticleRepository articleRepo,
        IKbFolderRepository folderRepo)
    {
        _articleRepo = articleRepo;
        _folderRepo  = folderRepo;
    }

    public async Task<Result<Guid>> Handle(CreateKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            if (cmd.FolderId.HasValue)
            {
                var folder = await _folderRepo.GetByIdAsync(cmd.FolderId.Value, ct);
                if (folder is null)
                    return Result<Guid>.Failure($"Folder {cmd.FolderId} not found.");
                if (folder.IsDeleted)
                    return Result<Guid>.Failure("Cannot create an article inside a deleted folder.");
            }

            var article = KbArticle.Create(
                ArticleTitle.Create(cmd.Title),
                cmd.Content,
                cmd.ArticleType,
                authorId:  cmd.ActorId,
                folderId:  cmd.FolderId,
                createdBy: cmd.ActorId);

            _articleRepo.Add(article);
            await _articleRepo.SaveChangesAsync(ct);

            return Result<Guid>.Success(article.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// ─── UpdateKbArticleCommandHandler ───────────────────────────────────────────

public sealed class UpdateKbArticleCommandHandler
    : IRequestHandler<UpdateKbArticleCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public UpdateKbArticleCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(UpdateKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null)
                return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.UpdateContent(ArticleTitle.Create(cmd.NewTitle), cmd.NewContent, cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

// ─── MoveKbArticleCommandHandler ─────────────────────────────────────────────

public sealed class MoveKbArticleCommandHandler
    : IRequestHandler<MoveKbArticleCommand, Result>
{
    private readonly IKbArticleRepository _articleRepo;
    private readonly IKbFolderRepository  _folderRepo;

    public MoveKbArticleCommandHandler(
        IKbArticleRepository articleRepo,
        IKbFolderRepository folderRepo)
    {
        _articleRepo = articleRepo;
        _folderRepo  = folderRepo;
    }

    public async Task<Result> Handle(MoveKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _articleRepo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null)
                return Result.Failure($"Article {cmd.ArticleId} not found.");

            if (cmd.TargetFolderId.HasValue)
            {
                var folder = await _folderRepo.GetByIdAsync(cmd.TargetFolderId.Value, ct);
                if (folder is null)
                    return Result.Failure($"Folder {cmd.TargetFolderId} not found.");
                if (folder.IsDeleted)
                    return Result.Failure("Cannot move article into a deleted folder.");
            }

            article.MoveToFolder(cmd.TargetFolderId, cmd.ActorId);
            _articleRepo.Update(article);
            await _articleRepo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

// ─── PublishKbArticleCommandHandler ──────────────────────────────────────────

public sealed class PublishKbArticleCommandHandler
    : IRequestHandler<PublishKbArticleCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public PublishKbArticleCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(PublishKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.Publish(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── ArchiveKbArticleCommandHandler ──────────────────────────────────────────

public sealed class ArchiveKbArticleCommandHandler
    : IRequestHandler<ArchiveKbArticleCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public ArchiveKbArticleCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(ArchiveKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.Archive(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── RestoreKbArticleToDraftCommandHandler ────────────────────────────────────

public sealed class RestoreKbArticleToDraftCommandHandler
    : IRequestHandler<RestoreKbArticleToDraftCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public RestoreKbArticleToDraftCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(RestoreKbArticleToDraftCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.RestoreToDraft(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── DeleteKbArticleCommandHandler ───────────────────────────────────────────

public sealed class DeleteKbArticleCommandHandler
    : IRequestHandler<DeleteKbArticleCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public DeleteKbArticleCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteKbArticleCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.SoftDelete(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
