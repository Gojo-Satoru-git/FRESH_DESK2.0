using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Persistence.Interceptors;

public sealed class LookupCacheInvalidationInterceptor : SaveChangesInterceptor
{
    private readonly ICacheService _cacheService;

    public LookupCacheInvalidationInterceptor(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null) return result;

        var entries = eventData.Context.ChangeTracker.Entries().ToList();
        
        var hasGeoRegionChanges = entries.Any(e => e.Entity is GeoRegion && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        var hasModuleChanges = entries.Any(e => e.Entity is Module && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        var hasSolutionTypeChanges = entries.Any(e => e.Entity is SolutionType && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        var hasCustomerTierChanges = entries.Any(e => e.Entity is CustomerTier && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        var hasProductVersionChanges = entries.Any(e => e.Entity is ProductVersion && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        if (hasGeoRegionChanges) await _cacheService.RemoveAsync("lookups:regions", cancellationToken);
        if (hasModuleChanges) await _cacheService.RemoveAsync("lookups:modules", cancellationToken);
        if (hasSolutionTypeChanges) await _cacheService.RemoveAsync("lookups:solution-types", cancellationToken);
        if (hasCustomerTierChanges) await _cacheService.RemoveAsync("lookups:customer-tiers", cancellationToken);
        if (hasProductVersionChanges) await _cacheService.RemoveAsync("lookups:product-versions", cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
