using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Lookup.Application.DTOs;
using Adrenalin.Modules.Lookup.Application.Queries;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Persistence.Repositories;

public sealed class LookupQueryService : ILookupQueryService
{
    private readonly AdrenalinDbContext _dbContext;
    private readonly ICacheService _cache;

    public LookupQueryService(AdrenalinDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyList<GeoRegionDto>> GetGeoRegionsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "lookups:regions",
            async () =>
            {
                return await _dbContext.GeoRegions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Label)
                    .Select(x => new GeoRegionDto(
                        x.Id,
                        x.Code,
                        x.Label,
                        x.Timezone,
                        x.BusinessStart,
                        x.BusinessEnd,
                        x.WorkingDays))
                    .ToListAsync(cancellationToken);
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<IReadOnlyList<ModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "lookups:modules",
            async () =>
            {
                return await _dbContext.Set<Adrenalin.Modules.Lookup.Domain.Entities.Module>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Code)
                    .Select(x => new ModuleDto(
                        x.Id,
                        x.Code,
                        x.Label,
                        x.Description,
                        x.IsActive))
                    .ToListAsync(cancellationToken);
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<IReadOnlyList<SolutionTypeDto>> GetSolutionTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "lookups:solution-types",
            async () =>
            {
                return await _dbContext.Set<Adrenalin.Modules.Lookup.Domain.Entities.SolutionType>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Code)
                    .Select(x => new SolutionTypeDto(
                        x.Id,
                        x.Code,
                        x.Label,
                        x.IsActive))
                    .ToListAsync(cancellationToken);
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerTierDto>> GetCustomerTiersAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "lookups:customer-tiers",
            async () =>
            {
                return await _dbContext.Set<Adrenalin.Modules.Lookup.Domain.Entities.CustomerTier>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.PriorityBump)
                    .Select(x => new CustomerTierDto(
                        x.Id,
                        x.Code,
                        x.Label,
                        x.Description,
                        x.PriorityBump))
                    .ToListAsync(cancellationToken);
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVersionDto>> GetProductVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "lookups:product-versions",
            async () =>
            {
                return await _dbContext.Set<Adrenalin.Modules.Lookup.Domain.Entities.ProductVersion>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .Select(x => new ProductVersionDto(
                        x.Id,
                        x.Code,
                        x.Label,
                        x.ReleaseDate,
                        x.IsLegacy))
                    .ToListAsync(cancellationToken);
            },
            TimeSpan.FromHours(24),
            cancellationToken);
    }
}
