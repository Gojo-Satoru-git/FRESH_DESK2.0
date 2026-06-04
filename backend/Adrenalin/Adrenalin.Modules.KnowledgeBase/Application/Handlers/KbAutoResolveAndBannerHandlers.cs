using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.Services;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.KB.Application.Handlers;

// ─── EnableAutoResolveCommandHandler ─────────────────────────────────────────

public sealed class EnableAutoResolveCommandHandler
    : IRequestHandler<EnableAutoResolveCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public EnableAutoResolveCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(EnableAutoResolveCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            if (article.Status != ArticleStatus.Published)
                return Result.Failure(
                    "Auto-resolve can only be enabled on a Published article. Publish it first.");

            article.EnableAutoResolve(
                cmd.Keywords.ToArray(),
                cmd.ResolutionText,
                ConfidenceThreshold.Create(cmd.ConfidenceThreshold),
                cmd.ActorId);

            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── DisableAutoResolveCommandHandler ────────────────────────────────────────

public sealed class DisableAutoResolveCommandHandler
    : IRequestHandler<DisableAutoResolveCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public DisableAutoResolveCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(DisableAutoResolveCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.DisableAutoResolve(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── MarkArticleAsGuardrailExcludedCommandHandler ────────────────────────────

public sealed class MarkArticleAsGuardrailExcludedCommandHandler
    : IRequestHandler<MarkArticleAsGuardrailExcludedCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public MarkArticleAsGuardrailExcludedCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(MarkArticleAsGuardrailExcludedCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.MarkAsGuardrailExcluded(cmd.ActorId);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── RecordArticleReopenedMatchCommandHandler (learning loop job) ─────────────

public sealed class RecordArticleReopenedMatchCommandHandler
    : IRequestHandler<RecordArticleReopenedMatchCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public RecordArticleReopenedMatchCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(RecordArticleReopenedMatchCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.RecordReopenedMatch(cmd.ThresholdRaiseDelta);
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── RecordArticleMatchCommandHandler ────────────────────────────────────────

public sealed class RecordArticleMatchCommandHandler
    : IRequestHandler<RecordArticleMatchCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public RecordArticleMatchCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(RecordArticleMatchCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetByIdAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.RecordMatch();
            _repo.Update(article);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── AddAttachmentCommandHandler ─────────────────────────────────────────────

public sealed class AddAttachmentCommandHandler
    : IRequestHandler<AddAttachmentCommand, Result<Guid>>
{
    private readonly IKbArticleRepository _repo;
    private readonly IKbFileStorageService _fileStorage;

    public AddAttachmentCommandHandler(
        IKbArticleRepository repo,
        IKbFileStorageService fileStorage)
    {
        _repo = repo;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Guid>> Handle(AddAttachmentCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetWithAttachmentsAsync(cmd.ArticleId, ct);
            if (article is null)
                return Result<Guid>.Failure($"Article {cmd.ArticleId} not found.");

            // Persist the file to the server and obtain its server-relative URL.
            var fileUrl = await _fileStorage.SaveAsync(
                cmd.ArticleId, cmd.FileName, cmd.FileStream, ct);

            var attachment = article.AddAttachment(
                fileUrl, cmd.FileName, cmd.FileSizeBytes, cmd.MimeType);

            // Explicitly tell EF this is a new row (INSERT).
            // Do NOT call _repo.Update(article) — that marks every tracked entity
            // as Modified, flipping the new attachment from Added → Modified and
            // causing EF to emit UPDATE on a non-existent row (0 rows → concurrency error).
            _repo.AddAttachment(attachment);
            await _repo.SaveChangesAsync(ct);

            return Result<Guid>.Success(attachment.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

// ─── RemoveAttachmentCommandHandler ──────────────────────────────────────────

public sealed class RemoveAttachmentCommandHandler
    : IRequestHandler<RemoveAttachmentCommand, Result>
{
    private readonly IKbArticleRepository _repo;

    public RemoveAttachmentCommandHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result> Handle(RemoveAttachmentCommand cmd, CancellationToken ct)
    {
        try
        {
            var article = await _repo.GetWithAttachmentsAsync(cmd.ArticleId, ct);
            if (article is null) return Result.Failure($"Article {cmd.ArticleId} not found.");

            article.RemoveAttachment(cmd.AttachmentId);

            // The attachment's IsDeleted flag is tracked as Modified by EF automatically.
            // No need to call _repo.Update(article) — that would re-stamp every
            // article column and can cause concurrency conflicts on the article row.
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ─── Portal Banner Handlers ───────────────────────────────────────────────────

public sealed class CreatePortalBannerCommandHandler
    : IRequestHandler<CreatePortalBannerCommand, Result<Guid>>
{
    private readonly IPortalBannerRepository _repo;

    public CreatePortalBannerCommandHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(CreatePortalBannerCommand cmd, CancellationToken ct)
    {
        try
        {
            var banner = Domain.Entities.PortalBanner.Create(
                cmd.Title, cmd.Message, cmd.ActiveFrom, cmd.ActiveTo, cmd.ActorId);

            _repo.Add(banner);
            await _repo.SaveChangesAsync(ct);

            return Result<Guid>.Success(banner.Id);
        }
        catch (Exception ex) { return Result<Guid>.Failure(ex.Message); }
    }
}

public sealed class UpdatePortalBannerCommandHandler
    : IRequestHandler<UpdatePortalBannerCommand, Result>
{
    private readonly IPortalBannerRepository _repo;

    public UpdatePortalBannerCommandHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result> Handle(UpdatePortalBannerCommand cmd, CancellationToken ct)
    {
        try
        {
            var banner = await _repo.GetByIdAsync(cmd.BannerId, ct);
            if (banner is null) return Result.Failure($"Banner {cmd.BannerId} not found.");

            banner.Update(cmd.Title, cmd.Message, cmd.ActiveFrom, cmd.ActiveTo, cmd.ActorId);
            _repo.Update(banner);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class ActivatePortalBannerCommandHandler
    : IRequestHandler<ActivatePortalBannerCommand, Result>
{
    private readonly IPortalBannerRepository _repo;

    public ActivatePortalBannerCommandHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result> Handle(ActivatePortalBannerCommand cmd, CancellationToken ct)
    {
        try
        {
            var banner = await _repo.GetByIdAsync(cmd.BannerId, ct);
            if (banner is null) return Result.Failure($"Banner {cmd.BannerId} not found.");

            banner.Activate(cmd.ActorId);
            _repo.Update(banner);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public sealed class DeactivatePortalBannerCommandHandler
    : IRequestHandler<DeactivatePortalBannerCommand, Result>
{
    private readonly IPortalBannerRepository _repo;

    public DeactivatePortalBannerCommandHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeactivatePortalBannerCommand cmd, CancellationToken ct)
    {
        try
        {
            var banner = await _repo.GetByIdAsync(cmd.BannerId, ct);
            if (banner is null) return Result.Failure($"Banner {cmd.BannerId} not found.");

            banner.Deactivate(cmd.ActorId);
            _repo.Update(banner);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}