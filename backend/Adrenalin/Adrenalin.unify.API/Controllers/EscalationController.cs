using Adrenalin.Modules.SLA.Application.Commands;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/escalations")]
public class EscalationController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public EscalationController(IDispatcher dispatcher)
        => _dispatcher = dispatcher;

    // Manual trigger for testing
    [HttpPost("check")]
    public async Task<IActionResult> CheckEscalations(
        CancellationToken ct)
    {
        var result = await _dispatcher.Send(
            new CheckEscalationsCommand(), ct);

        return result.IsSuccess
            ? Ok(new
            {
                message = "Escalation check complete",
                ticketsEscalated = result.Value
            })
            : BadRequest(new { error = result.Error });
    }
}