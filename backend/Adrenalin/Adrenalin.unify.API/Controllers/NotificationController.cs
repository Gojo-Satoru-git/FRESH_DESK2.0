using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Notification.Application.Queries;
using Microsoft.AspNetCore.Authorization;

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
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetNotificationHistory(CancellationToken ct)
    {
        string currentUserEmail = User.FindFirst("email")?.Value
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                  ?? string.Empty;

        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return Unauthorized(new { Error = "Could not resolve user identity boundary." });
        }

        // ✅ Fixed: Dispatch a query instead of querying a repository directly!
        var query = new GetNotificationHistoryQuery(currentUserEmail.Trim());
        var logs = await _dispatcher.Send(query, ct);

        var formattedLogs = logs.Select(log => new
        {
            log.Id,
            log.TemplateId,
            log.RecipientEmail,
            log.SentAt,
            log.IsFailedDelivery,
            log.TicketNumber,
            log.TicketId,
            Message = log.ErrorMessage
        });

        return Ok(formattedLogs);
    }
    [HttpGet("unread")]
    [Authorize]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken ct)
    {
     
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
        var formattedLogs = logs.Select(log => new
        {
            log.Id,
            log.TemplateId,
            log.RecipientEmail,
            log.SentAt,
            log.IsFailedDelivery,
            // ✅ Maps your database "errorMessage" value onto a generic "message" tag
            Message = log.ErrorMessage
        });
        return Ok(formattedLogs);
    }
}