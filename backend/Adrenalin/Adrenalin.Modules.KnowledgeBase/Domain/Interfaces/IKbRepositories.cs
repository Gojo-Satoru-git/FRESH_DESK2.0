using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Enums;

namespace Adrenalin.Modules.KB.Domain.Interfaces;

// ─── IKbFolderRepository ──────────────────────────────────────────────────────

public interface IKbFolderRepository
{
    Task<KbFolder?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>All root folders (parent_id IS NULL, not deleted), ordered by display_order.</summary>
    Task<IReadOnlyList<KbFolder>> GetRootFoldersAsync(CancellationToken ct = default);

    /// <summary>
    /// Full recursive subtree for a given root via WITH RECURSIVE CTE.
    /// Returns every non-deleted descendant including the root itself.
    /// </summary>
    Task<IReadOnlyList<KbFolder>> GetSubtreeAsync(Guid rootFolderId, CancellationToken ct = default);

    /// <summary>Depth of the folder's ancestry chain (root = 0).</summary>
    Task<int> GetDepthAsync(Guid folderId, CancellationToken ct = default);

    /// <summary>True if the folder has any non-deleted articles.</summary>
    Task<bool> HasArticlesAsync(Guid folderId, CancellationToken ct = default);

    void Add(KbFolder folder);
    void Update(KbFolder folder);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─── IKbArticleRepository ─────────────────────────────────────────────────────

public interface IKbArticleRepository
{
    Task<KbArticle?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Article with all non-deleted attachments loaded.</summary>
    Task<KbArticle?> GetWithAttachmentsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Paginated search — uses the GIN trigram index on title when titleQuery is set.
    /// All filters optional; defaults: page 1, 20 items.
    /// </summary>
    Task<(IReadOnlyList<KbArticle> Items, int TotalCount)> SearchAsync(
        string? titleQuery,
        ArticleType? articleType,
        ArticleStatus? status,
        Guid? folderId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Published articles with auto_resolve=true and guardrail_excluded=false.
    /// Used by the auto-resolution engine to warm its candidate list at startup.
    /// </summary>
    Task<IReadOnlyList<KbArticle>> GetAutoResolveCandidatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Phase-1 keyword match: returns auto-resolve candidates whose keywords
    /// overlap with <paramref name="keywords"/>. Uses the GIN array index.
    /// </summary>
    Task<IReadOnlyList<KbArticle>> FindByKeywordsAsync(
        IReadOnlyList<string> keywords,
        CancellationToken ct = default);

    void Add(KbArticle article);
    void Update(KbArticle article);
    void AddAttachment(KbAttachment attachment);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─── IPortalBannerRepository ──────────────────────────────────────────────────

public interface IPortalBannerRepository
{
    Task<PortalBanner?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Active banners within the schedule window — used by portal-facing API.
    /// The SQL WHERE clause mirrors PortalBanner.IsCurrentlyVisible().
    /// </summary>
    Task<IReadOnlyList<PortalBanner>> GetActiveBannersAsync(
        DateTimeOffset now, CancellationToken ct = default);

    /// <summary>All banners (admin management list).</summary>
    Task<IReadOnlyList<PortalBanner>> GetAllAsync(CancellationToken ct = default);

    void Add(PortalBanner banner);
    void Update(PortalBanner banner);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}