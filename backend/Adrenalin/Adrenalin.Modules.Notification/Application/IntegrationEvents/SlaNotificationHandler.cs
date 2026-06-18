using Adrenalin.SharedKernel.Interfaces;
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
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;

namespace Adrenalin.Modules.Notification.Application.IntegrationEvents;

public sealed class SlaNotificationHandler : IIntegrationEventHandler<SlaBreachedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<SlaNotificationHandler> _logger;
    private readonly IEmailService _emailService;

    public SlaNotificationHandler(INotificationRepository repository, ILogger<SlaNotificationHandler> logger, IEmailService emailService)
    {
        _repository = repository;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task HandleAsync(SlaBreachedIntegrationEvent notification, CancellationToken cancellationToken)
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

        var companyId = await _repository.GetCompanyIdByTicketIdAsync(notification.TicketId, cancellationToken);
        var resolvedEmails = await _repository.ResolveEmailsAsync(notification.TargetUserIds, cancellationToken);

        // 🎯 REVERTED: Restored the database user email address lookup routine
        var logs = new List<NotificationLog>();
        foreach (var userId in notification.TargetUserIds)
        {
            if (!resolvedEmails.TryGetValue(userId, out var userEmail))
            {
                userEmail = $"inapp_user_{userId:N}@adrenalin.internal";
            }

            var log = new NotificationLog
            {
                CompanyId = companyId,
                TicketId = notification.TicketId,
                TicketNumber = notification.TicketNumber,
                RecipientEmail = userEmail, // Saves real emails (e.g., agent2@adrenalin.org)
                ErrorMessage = null,
                IsFailedDelivery = false,
                SentAt = DateTime.UtcNow,
                TemplateId = template.Id
            };

            logs.Add(log);
            await _emailService.SendAsync(userEmail, subject, body);
        }
        await _repository.AddLogsAsync(logs, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged SLA Breach notification successfully for Ticket {TicketId} to {Count} recipients",
            notification.TicketId, notification.TargetUserIds.Count);
    }
}