using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.KnowledgeBase;

public sealed class PortalBannerRepository : IPortalBannerRepository
{
    private readonly AdrenalinDbContext _ctx;

    public PortalBannerRepository(AdrenalinDbContext ctx) => _ctx = ctx;

    public async Task<PortalBanner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.PortalBanners
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<PortalBanner>> GetActiveBannersAsync(
        DateTimeOffset now, CancellationToken ct = default)
        => await _ctx.PortalBanners
            .Where(b =>
                b.IsActive &&
                (b.ActiveFrom == null || b.ActiveFrom <= now) &&
                (b.ActiveTo   == null || b.ActiveTo   >= now))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PortalBanner>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.PortalBanners
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public void Add(PortalBanner banner)    => _ctx.PortalBanners.Add(banner);
    public void Update(PortalBanner banner) => _ctx.PortalBanners.Update(banner);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);
}
