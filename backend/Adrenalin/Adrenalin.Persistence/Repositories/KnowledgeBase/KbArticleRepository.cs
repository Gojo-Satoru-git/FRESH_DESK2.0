using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.KnowledgeBase;

public sealed class KbArticleRepository : IKbArticleRepository
{
    private readonly AdrenalinDbContext _ctx;

    public KbArticleRepository(AdrenalinDbContext ctx) => _ctx = ctx;

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<KbArticle?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.KbArticles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<KbArticle?> GetWithAttachmentsAsync(Guid id, CancellationToken ct = default)
        => await _ctx.KbArticles
            .IgnoreQueryFilters()
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<(IReadOnlyList<KbArticle> Items, int TotalCount)> SearchAsync(
        string? titleQuery,
        ArticleType? articleType,
        ArticleStatus? status,
        Guid? folderId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _ctx.KbArticles.AsQueryable(); // query filter applies (no deleted)

        if (!string.IsNullOrWhiteSpace(titleQuery))
        {
            // EF.Functions.ILike leverages the GIN trigram index on title
            query = query.Where(a =>
                EF.Functions.ILike(a.Title, $"%{titleQuery}%"));
        }

        if (articleType.HasValue)
            query = query.Where(a => a.ArticleType == articleType.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (folderId.HasValue)
            query = query.Where(a => a.FolderId == folderId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<KbArticle>> GetAutoResolveCandidatesAsync(
        CancellationToken ct = default)
        => await _ctx.KbArticles
            .Where(a =>
                a.Status == ArticleStatus.Published &&
                a.AutoResolve &&
                !a.GuardrailExcluded)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<KbArticle>> FindByKeywordsAsync(
        IReadOnlyList<string> keywords, CancellationToken ct = default)
    {
        // Uses Postgres array overlap operator (&&) via raw SQL for GIN index support.
        // EF Core does not generate && natively for string arrays, so we use FromSqlRaw.
        var keywordArray = keywords.ToArray();

        return await _ctx.KbArticles
            .FromSqlRaw(@"
                SELECT * FROM kb.kb_articles
                WHERE  is_deleted        = false
                  AND  status            = 'published'
                  AND  auto_resolve      = true
                  AND  guardrail_excluded = false
                  AND  keywords && {0}::text[]",
                (object)keywordArray)
            .ToListAsync(ct);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public void Add(KbArticle article) => _ctx.KbArticles.Add(article);
    public void Update(KbArticle article) => _ctx.KbArticles.Update(article);
    public void AddAttachment(KbAttachment attachment) => _ctx.Set<KbAttachment>().Add(attachment);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);
}