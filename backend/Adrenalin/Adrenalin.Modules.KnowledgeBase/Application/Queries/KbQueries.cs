using Adrenalin.Modules.KB.Application.DTOs;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.KB.Application.Queries;

// ─── Folder ───────────────────────────────────────────────────────────────────

public sealed record GetKbFolderByIdQuery(Guid FolderId)
    : IRequest<Result<KbFolderDto>>;

public sealed record GetKbFolderTreeQuery()
    : IRequest<Result<IReadOnlyList<KbFolderTreeNodeDto>>>;

public sealed record GetKbFolderChildrenQuery(Guid ParentFolderId)
    : IRequest<Result<IReadOnlyList<KbFolderDto>>>;

// ─── Article ──────────────────────────────────────────────────────────────────

public sealed record GetKbArticleByIdQuery(Guid ArticleId)
    : IRequest<Result<KbArticleDto>>;

public sealed record GetKbArticleWithAttachmentsQuery(Guid ArticleId)
    : IRequest<Result<KbArticleWithAttachmentsDto>>;

/// <summary>
/// Full-text search using the GIN trigram index on kb.kb_articles.title.
/// All filters optional.
/// </summary>
public sealed record SearchKbArticlesQuery(
    string? TitleQuery  = null,
    ArticleType? ArticleType = null,
    ArticleStatus? Status    = null,
    Guid? FolderId           = null,
    int PageNumber           = 1,
    int PageSize             = 20)
    : IRequest<Result<KbArticleSearchResultDto>>;

public sealed record GetArticlesByFolderQuery(Guid FolderId)
    : IRequest<Result<IReadOnlyList<KbArticleSummaryDto>>>;

/// <summary>
/// Returns all auto-resolve candidates (published, auto_resolve=true, not guardrail).
/// Used by the AI engine to warm its candidate list.
/// </summary>
public sealed record GetAutoResolveCandidatesQuery()
    : IRequest<Result<IReadOnlyList<KbArticleDto>>>;

// ─── Portal Banner ────────────────────────────────────────────────────────────

public sealed record GetActivePortalBannersQuery()
    : IRequest<Result<IReadOnlyList<ActivePortalBannerDto>>>;

public sealed record GetAllPortalBannersQuery()
    : IRequest<Result<IReadOnlyList<PortalBannerDto>>>;

public sealed record GetPortalBannerByIdQuery(Guid BannerId)
    : IRequest<Result<PortalBannerDto>>;
