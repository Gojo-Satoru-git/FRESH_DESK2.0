using Adrenalin.Modules.KB.Application.DTOs;
using Adrenalin.Modules.KB.Application.Queries;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.KB.Application.Handlers;

// ─── Folder Query Handlers ────────────────────────────────────────────────────

public sealed class GetKbFolderByIdQueryHandler
    : IRequestHandler<GetKbFolderByIdQuery, Result<KbFolderDto>>
{
    private readonly IKbFolderRepository _repo;
    public GetKbFolderByIdQueryHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result<KbFolderDto>> Handle(GetKbFolderByIdQuery q, CancellationToken ct)
    {
        var folder = await _repo.GetByIdAsync(q.FolderId, ct);
        if (folder is null || folder.IsDeleted)
            return Result<KbFolderDto>.Failure($"Folder {q.FolderId} not found.");

        return Result<KbFolderDto>.Success(MapFolder(folder));
    }

    internal static KbFolderDto MapFolder(KbFolder f) =>
        new(f.Id, f.Name, f.ParentId, f.DisplayOrder, f.CreatedAt, f.UpdatedAt);
}

public sealed class GetKbFolderTreeQueryHandler
    : IRequestHandler<GetKbFolderTreeQuery, Result<IReadOnlyList<KbFolderTreeNodeDto>>>
{
    private readonly IKbFolderRepository _repo;
    public GetKbFolderTreeQueryHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<KbFolderTreeNodeDto>>> Handle(
        GetKbFolderTreeQuery q, CancellationToken ct)
    {
        var roots = await _repo.GetRootFoldersAsync(ct);
        var tree  = new List<KbFolderTreeNodeDto>();

        foreach (var root in roots)
        {
            var subtree = await _repo.GetSubtreeAsync(root.Id, ct);
            tree.Add(BuildNode(root, subtree.ToList()));
        }

        return Result<IReadOnlyList<KbFolderTreeNodeDto>>.Success(tree);
    }

    private static KbFolderTreeNodeDto BuildNode(KbFolder folder, List<KbFolder> all)
    {
        var children = all
            .Where(f => f.ParentId == folder.Id)
            .OrderBy(f => f.DisplayOrder)
            .Select(child => BuildNode(child, all))
            .ToList();

        return new KbFolderTreeNodeDto(
            folder.Id, folder.Name, folder.ParentId, folder.DisplayOrder, children);
    }
}

public sealed class GetKbFolderChildrenQueryHandler
    : IRequestHandler<GetKbFolderChildrenQuery, Result<IReadOnlyList<KbFolderDto>>>
{
    private readonly IKbFolderRepository _repo;
    public GetKbFolderChildrenQueryHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<KbFolderDto>>> Handle(
        GetKbFolderChildrenQuery q, CancellationToken ct)
    {
        var subtree = await _repo.GetSubtreeAsync(q.ParentFolderId, ct);

        var children = subtree
            .Where(f => f.ParentId == q.ParentFolderId)
            .OrderBy(f => f.DisplayOrder)
            .Select(GetKbFolderByIdQueryHandler.MapFolder)
            .ToList();

        return Result<IReadOnlyList<KbFolderDto>>.Success(children);
    }
}

// ─── Article Query Handlers ───────────────────────────────────────────────────

public sealed class GetKbArticleByIdQueryHandler
    : IRequestHandler<GetKbArticleByIdQuery, Result<KbArticleDto>>
{
    private readonly IKbArticleRepository _repo;
    public GetKbArticleByIdQueryHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result<KbArticleDto>> Handle(GetKbArticleByIdQuery q, CancellationToken ct)
    {
        var article = await _repo.GetByIdAsync(q.ArticleId, ct);
        if (article is null || article.IsDeleted)
            return Result<KbArticleDto>.Failure($"Article {q.ArticleId} not found.");

        return Result<KbArticleDto>.Success(MapArticle(article));
    }

    internal static KbArticleDto MapArticle(KbArticle a) => new(
        a.Id, a.Title, a.Content, a.ArticleType, a.Status, a.IsPublished,
        a.AuthorId, a.FolderId,
        a.AutoResolve, a.ConfidenceThresholdValue, a.Keywords,
        a.ResolutionText, a.GuardrailExcluded,
        a.TimesMatched, a.TimesReopened,
        a.CreatedAt, a.UpdatedAt);

    internal static KbArticleSummaryDto MapSummary(KbArticle a) => new(
        a.Id, a.Title, a.ArticleType, a.Status,
        a.IsPublished, a.AutoResolve, a.GuardrailExcluded,
        a.FolderId, a.UpdatedAt);
}

public sealed class GetKbArticleWithAttachmentsQueryHandler
    : IRequestHandler<GetKbArticleWithAttachmentsQuery, Result<KbArticleWithAttachmentsDto>>
{
    private readonly IKbArticleRepository _repo;
    public GetKbArticleWithAttachmentsQueryHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result<KbArticleWithAttachmentsDto>> Handle(
        GetKbArticleWithAttachmentsQuery q, CancellationToken ct)
    {
        var article = await _repo.GetWithAttachmentsAsync(q.ArticleId, ct);
        if (article is null || article.IsDeleted)
            return Result<KbArticleWithAttachmentsDto>.Failure($"Article {q.ArticleId} not found.");

        var attachments = article.Attachments
            .Where(a => !a.IsDeleted)
            .Select(a => new KbAttachmentDto(
                a.Id, a.ArticleId, a.FileUrl, a.FileName,
                a.FileSizeBytes, a.MimeType, a.CreatedAt))
            .ToList();

        return Result<KbArticleWithAttachmentsDto>.Success(
            new KbArticleWithAttachmentsDto(
                GetKbArticleByIdQueryHandler.MapArticle(article), attachments));
    }
}

public sealed class SearchKbArticlesQueryHandler
    : IRequestHandler<SearchKbArticlesQuery, Result<KbArticleSearchResultDto>>
{
    private readonly IKbArticleRepository _repo;
    public SearchKbArticlesQueryHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result<KbArticleSearchResultDto>> Handle(
        SearchKbArticlesQuery q, CancellationToken ct)
    {
        var (items, total) = await _repo.SearchAsync(
            q.TitleQuery, q.ArticleType, q.Status,
            q.FolderId, q.PageNumber, q.PageSize, ct);

        var summaries = items
            .Select(GetKbArticleByIdQueryHandler.MapSummary)
            .ToList();

        return Result<KbArticleSearchResultDto>.Success(
            new KbArticleSearchResultDto(summaries, total, q.PageNumber, q.PageSize));
    }
}

public sealed class GetArticlesByFolderQueryHandler
    : IRequestHandler<GetArticlesByFolderQuery, Result<IReadOnlyList<KbArticleSummaryDto>>>
{
    private readonly IKbArticleRepository _repo;
    public GetArticlesByFolderQueryHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<KbArticleSummaryDto>>> Handle(
        GetArticlesByFolderQuery q, CancellationToken ct)
    {
        var (items, _) = await _repo.SearchAsync(
            null, null, ArticleStatus.Published,
            q.FolderId, 1, 200, ct);

        var result = items
            .Select(GetKbArticleByIdQueryHandler.MapSummary)
            .ToList();

        return Result<IReadOnlyList<KbArticleSummaryDto>>.Success(result);
    }
}

public sealed class GetAutoResolveCandidatesQueryHandler
    : IRequestHandler<GetAutoResolveCandidatesQuery, Result<IReadOnlyList<KbArticleDto>>>
{
    private readonly IKbArticleRepository _repo;
    public GetAutoResolveCandidatesQueryHandler(IKbArticleRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<KbArticleDto>>> Handle(
        GetAutoResolveCandidatesQuery q, CancellationToken ct)
    {
        var articles = await _repo.GetAutoResolveCandidatesAsync(ct);
        var dtos = articles.Select(GetKbArticleByIdQueryHandler.MapArticle).ToList();
        return Result<IReadOnlyList<KbArticleDto>>.Success(dtos);
    }
}

// ─── Portal Banner Query Handlers ─────────────────────────────────────────────

public sealed class GetActivePortalBannersQueryHandler
    : IRequestHandler<GetActivePortalBannersQuery, Result<IReadOnlyList<ActivePortalBannerDto>>>
{
    private readonly IPortalBannerRepository _repo;
    public GetActivePortalBannersQueryHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ActivePortalBannerDto>>> Handle(
        GetActivePortalBannersQuery q, CancellationToken ct)
    {
        var now     = DateTimeOffset.UtcNow;
        var banners = await _repo.GetActiveBannersAsync(now, ct);

        var dtos = banners
            .Where(b => b.IsCurrentlyVisible(now))
            .Select(b => new ActivePortalBannerDto(b.Id, b.Title, b.Message))
            .ToList();

        return Result<IReadOnlyList<ActivePortalBannerDto>>.Success(dtos);
    }
}

public sealed class GetAllPortalBannersQueryHandler
    : IRequestHandler<GetAllPortalBannersQuery, Result<IReadOnlyList<PortalBannerDto>>>
{
    private readonly IPortalBannerRepository _repo;
    public GetAllPortalBannersQueryHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<PortalBannerDto>>> Handle(
        GetAllPortalBannersQuery q, CancellationToken ct)
    {
        var banners = await _repo.GetAllAsync(ct);
        var dtos    = banners.Select(MapBanner).ToList();
        return Result<IReadOnlyList<PortalBannerDto>>.Success(dtos);
    }

    internal static PortalBannerDto MapBanner(Domain.Entities.PortalBanner b) =>
        new(b.Id, b.Title, b.Message, b.ActiveFrom, b.ActiveTo,
            b.IsActive, b.CreatedAt, b.UpdatedAt);
}

public sealed class GetPortalBannerByIdQueryHandler
    : IRequestHandler<GetPortalBannerByIdQuery, Result<PortalBannerDto>>
{
    private readonly IPortalBannerRepository _repo;
    public GetPortalBannerByIdQueryHandler(IPortalBannerRepository repo) => _repo = repo;

    public async Task<Result<PortalBannerDto>> Handle(
        GetPortalBannerByIdQuery q, CancellationToken ct)
    {
        var banner = await _repo.GetByIdAsync(q.BannerId, ct);
        if (banner is null)
            return Result<PortalBannerDto>.Failure($"Banner {q.BannerId} not found.");

        return Result<PortalBannerDto>.Success(
            GetAllPortalBannersQueryHandler.MapBanner(banner));
    }
}
