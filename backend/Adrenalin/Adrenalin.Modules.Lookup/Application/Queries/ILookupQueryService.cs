using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Lookup.Application.DTOs;

namespace Adrenalin.Modules.Lookup.Application.Queries;

public interface ILookupQueryService
{
    Task<IReadOnlyList<GeoRegionDto>> GetGeoRegionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SolutionTypeDto>> GetSolutionTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerTierDto>> GetCustomerTiersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVersionDto>> GetProductVersionsAsync(CancellationToken cancellationToken = default);
}
