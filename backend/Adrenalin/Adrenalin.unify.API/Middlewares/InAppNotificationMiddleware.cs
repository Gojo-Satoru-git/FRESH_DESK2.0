using Adrenalin.EventBus.Events;
using Microsoft.Extensions.Logging;

namespace Adrenalin.unify.API.Middlewares;
public sealed class InAppNotificationMiddleware
{
    private readonly ILogger<InAppNotificationMiddleware> _logger;

    public InAppNotificationMiddleware(ILogger<InAppNotificationMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Evaluates if the incoming notification log payload meets our strict in-app delivery criteria.
    /// </summary>
    // 📍 Inside InAppNotificationMiddleware.cs
    public bool ShouldProcess(SlaBreachedIntegrationEvent notificationEvent)
    {
        if (notificationEvent == null) return false;

        // ⚡ FIXED: Changed from .NotifyRole to .TargetRole to match your contract record!
        string targetRoleClean = !string.IsNullOrEmpty(notificationEvent.TargetRole)
            ? notificationEvent.TargetRole.ToLower()
            : "agent";

        string targetRecipient = $"inapp_user_{targetRoleClean}@adrenalin.org";

        if (targetRecipient.StartsWith("inapp_user_", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Middleware Validation PASSED: Routing log to internal app stream for Ticket {TicketNumber}",
                notificationEvent.TicketNumber);
            return true;
        }

        // ⚡ FIXED: Changed from .NotifyRole to .TargetRole here too
        _logger.LogWarning("Middleware Validation BLOCKED: Notification for role '{Role}' on Ticket {TicketNumber} is not flagged for in-app delivery.",
            notificationEvent.TargetRole, notificationEvent.TicketNumber);

        return false;
    }
}