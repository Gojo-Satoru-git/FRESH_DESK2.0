using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Notification.Application.Queries;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public NotificationController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken ct)
    {
        // ⚡ STEP 1: Safely read the user's validated email address from the incoming JWT identity claims
        // This checks both standard WS-Security claim namespaces and lightweight custom OAuth keys
        string currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value
                                  ?? User.FindFirst("email")?.Value
                                  ?? string.Empty;

        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return Unauthorized("Could not resolve user identity boundary from authorization token claims.");
        }

        // ⚡ STEP 2: Feed the resolved recipient email parameter straight into the query contract
        // This satisfies the compiler and resolves error CS7036!
        var query = new GetUnreadNotificationsQuery(currentUserEmail);

        var logs = await _dispatcher.Send(query, ct);
        return Ok(logs);
    }
}