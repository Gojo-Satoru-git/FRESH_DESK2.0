using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.IntegrationEvents;

public sealed class SlaNotificationHandler : INotificationHandler<SlaBreachNotificationContract>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<SlaNotificationHandler> _logger;

    public SlaNotificationHandler(INotificationRepository repository, ILogger<SlaNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(SlaBreachNotificationContract notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing SlaNotificationHandler for Ticket {TicketId}. Breach Type: {BreachType}",
            notification.TicketId, notification.BreachType);

        var template = await _repository.GetTemplateByCodeAsync("SLA_BREACH_ALERT", cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email/InApp template SLA_BREACH_ALERT not found. Aborting SLA log registration.");
            return;
        }

        var subject = template.Subject?
            .Replace("{{sla.breach_type}}", notification.BreachType)
            .Replace("{{sla.rule_name}}", notification.EscalationRuleName) ?? string.Empty;

        var body = template.BodyHtml?
            .Replace("{{ticket.id}}", notification.TicketId.ToString())
            .Replace("{{sla.breach_type}}", notification.BreachType)
            .Replace("{{sla.rule_name}}", notification.EscalationRuleName)
            .Replace("{{sla.target_role}}", notification.TargetRole)
            .Replace("{{ticket.url}}", $"http://localhost:4200/agent/tickets") ?? string.Empty;

        if (notification.TargetUserIds == null || notification.TargetUserIds.Count == 0)
        {
            _logger.LogWarning("No users found to notify for SLA Breach on Ticket {TicketId}", notification.TicketId);
            return;
        }

        // 🎯 REVERTED: Restored the database user email address lookup routine
        foreach (var userId in notification.TargetUserIds)
        {
            var userEmail = await _repository.ResolveEmailAsync(userId, cancellationToken);

            if (string.IsNullOrEmpty(userEmail))
            {
                userEmail = $"inapp_user_{userId:N}@adrenalin.internal";
            }

            var log = new NotificationLog
            {
                TicketId = notification.TicketId,
                TicketNumber = notification.TicketNumber,
                RecipientEmail = userEmail, // Saves real emails (e.g., agent2@adrenalin.org)
                ErrorMessage = null,
                IsFailedDelivery = false,
                SentAt = DateTime.UtcNow,
                TemplateId = template.Id
            };

            await _repository.AddLogAsync(log, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged SLA Breach notification successfully for Ticket {TicketId} to {Count} recipients",
            notification.TicketId, notification.TargetUserIds.Count);
    }
}