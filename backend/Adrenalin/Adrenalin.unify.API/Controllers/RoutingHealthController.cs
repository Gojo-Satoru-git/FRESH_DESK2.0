using Microsoft.AspNetCore.Mvc;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Ticketing.Application.Queries.Routing;
using Microsoft.AspNetCore.Authorization;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/routing-health")]
[AllowAnonymous]
public class RoutingHealthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public RoutingHealthController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.Send(new GetRoutingHealthQuery(), cancellationToken);
        
        if (!result.IsSuccess || result.Value == null)
            return StatusCode(500, "Failed to compute routing health");

        var payload = result.Value;
        
        if (payload.Status == "Critical")
        {
            return StatusCode(503, payload);
        }
        
        return Ok(payload);
    }
}
