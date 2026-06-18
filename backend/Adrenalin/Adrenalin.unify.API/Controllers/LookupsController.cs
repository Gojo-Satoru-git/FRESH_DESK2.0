using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Adrenalin.Modules.Lookup.Application.Queries;
using Adrenalin.Modules.Lookup.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupQueryService _lookupQueryService;

    public LookupsController(ILookupQueryService lookupQueryService)
    {
        _lookupQueryService = lookupQueryService;
    }

    [HttpGet("regions")]
    [ProducesResponseType(typeof(IReadOnlyList<GeoRegionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGeoRegions(CancellationToken cancellationToken)
    {
        var result = await _lookupQueryService.GetGeoRegionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("modules")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModules(CancellationToken cancellationToken)
    {
        var result = await _lookupQueryService.GetModulesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("solution-types")]
    [ProducesResponseType(typeof(IReadOnlyList<SolutionTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSolutionTypes(CancellationToken cancellationToken)
    {
        var result = await _lookupQueryService.GetSolutionTypesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("customer-tiers")]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerTierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerTiers(CancellationToken cancellationToken)
    {
        var result = await _lookupQueryService.GetCustomerTiersAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("product-versions")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductVersionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductVersions(CancellationToken cancellationToken)
    {
        var result = await _lookupQueryService.GetProductVersionsAsync(cancellationToken);
        return Ok(result);
    }
}
