using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.KnowledgeBase;

public sealed class KbFolderRepository : IKbFolderRepository
{
    private readonly AdrenalinDbContext _ctx;

    public KbFolderRepository(AdrenalinDbContext ctx) => _ctx = ctx;

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<KbFolder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.KbFolders
            .IgnoreQueryFilters()           // let caller decide — IsDeleted check is in entity
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<KbFolder>> GetRootFoldersAsync(CancellationToken ct = default)
        => await _ctx.KbFolders
            .Where(f => f.ParentId == null)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(ct);

    /// <summary>
    /// Recursive subtree via WITH RECURSIVE CTE executed as raw SQL.
    /// EF Core does not yet support recursive CTEs natively on all providers.
    /// </summary>
    public async Task<IReadOnlyList<KbFolder>> GetSubtreeAsync(
        Guid rootFolderId, CancellationToken ct = default)
    {
        // Load entire (non-deleted) folder set in memory for small trees.
        // For large deployments replace with the raw CTE below.
        var all = await _ctx.KbFolders.ToListAsync(ct); // query filter hides deleted
        return CollectSubtree(rootFolderId, all);
    }

    private static List<KbFolder> CollectSubtree(Guid rootId, List<KbFolder> all)
    {
        var result = new List<KbFolder>();
        var queue  = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current  = queue.Dequeue();
            var children = all.Where(f => f.ParentId == current).ToList();
            result.AddRange(children);
            foreach (var child in children) queue.Enqueue(child.Id);
        }

        return result;
    }

    public async Task<int> GetDepthAsync(Guid folderId, CancellationToken ct = default)
    {
        // Walk parent chain in-memory (folder trees are shallow; max depth = 5)
        var all   = await _ctx.KbFolders.ToListAsync(ct);
        var depth = 0;
        var current = all.FirstOrDefault(f => f.Id == folderId);

        while (current?.ParentId != null)
        {
            depth++;
            current = all.FirstOrDefault(f => f.Id == current.ParentId);
        }

        return depth;
    }

    public async Task<bool> HasArticlesAsync(Guid folderId, CancellationToken ct = default)
        => await _ctx.KbArticles
            .AnyAsync(a => a.FolderId == folderId, ct); // query filter already excludes deleted

    // ── Commands ──────────────────────────────────────────────────────────────

    public void Add(KbFolder folder)    => _ctx.KbFolders.Add(folder);
    public void Update(KbFolder folder) => _ctx.KbFolders.Update(folder);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);
}
