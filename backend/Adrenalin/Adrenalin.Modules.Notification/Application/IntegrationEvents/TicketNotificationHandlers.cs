using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.IntegrationEvents;

public sealed class TicketCreatedNotificationHandler : IIntegrationEventHandler<TicketCreatedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<TicketCreatedNotificationHandler> _logger;

    public TicketCreatedNotificationHandler(INotificationRepository repository, ILogger<TicketCreatedNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(TicketCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TicketCreatedNotificationHandler for Ticket {TicketId}", integrationEvent.TicketId);

        // 1. Notify Reporter
        var template = await _repository.GetTemplateByCodeAsync("TICKET_CREATED_REQUESTER", cancellationToken);

        if (template != null)
        {
            var subject = template.Subject?
                .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
                .Replace("{{ticket.subject}}", integrationEvent.Title) ?? string.Empty;

            var body = template.BodyHtml?
                .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
                .Replace("{{ticket.subject}}", integrationEvent.Title)
                .Replace("{{ticket.description}}", integrationEvent.Title)
                .Replace("{{ticket.priority}}", integrationEvent.Priority)
                .Replace("{{ticket.url}}", $"http://localhost:4200/customer-portal/ticket/{integrationEvent.TicketId}") ?? string.Empty;

            string? reporterEmail = null;
            if (integrationEvent.ReporterId.HasValue)
            {
                reporterEmail = await _repository.ResolveEmailAsync(integrationEvent.ReporterId.Value, cancellationToken);
            }
            if (string.IsNullOrEmpty(reporterEmail))
            {
                reporterEmail = "customer@company.com";
            }

            var log = new NotificationLog
            {
                TicketId = integrationEvent.TicketId,
                RecipientEmail = reporterEmail,
                ErrorMessage = null,
                IsFailedDelivery = false,
                SentAt = DateTime.UtcNow,
                TemplateId = template.Id
            };

            await _repository.AddLogAsync(log, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Email template TICKET_CREATED_REQUESTER not found.");
        }

        // 2. Notify Assignee (if assigned)
        if (integrationEvent.AssigneeId.HasValue)
        {
            var assigneeEmail = await _repository.ResolveEmailAsync(integrationEvent.AssigneeId.Value, cancellationToken);
            if (!string.IsNullOrEmpty(assigneeEmail))
            {
                var agentTemplate = await _repository.GetTemplateByCodeAsync("TICKET_ASSIGNED_AGENT", cancellationToken);
                if (agentTemplate != null)
                {
                    var agentSubject = agentTemplate.Subject?
                        .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
                        .Replace("{{ticket.priority}}", integrationEvent.Priority)
                        .Replace("{{ticket.company.name}}", "Adrenalin HRMS") ?? string.Empty;

                    var agentBody = agentTemplate.BodyHtml?
                        .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
                        .Replace("{{ticket.priority}}", integrationEvent.Priority)
                        .Replace("{{ticket.agent.name}}", "Agent")
                        .Replace("{{ticket.url}}", $"http://localhost:4200/agent/tickets") ?? string.Empty;

                    var agentLog = new NotificationLog
                    {
                        TicketId = integrationEvent.TicketId,
                        RecipientEmail = assigneeEmail,
                        ErrorMessage = null,
                        IsFailedDelivery = false,
                        SentAt = DateTime.UtcNow,
                        TemplateId = agentTemplate.Id
                    };
                    await _repository.AddLogAsync(agentLog, cancellationToken);
                }
            }
        }

        // 3. Notify Team Leads (Supervisors)
        var teamLeads = await _repository.GetTeamLeadsEmailsAsync(cancellationToken);
        if (teamLeads != null && teamLeads.Count > 0)
        {
            if (template != null)
            {
                foreach (var tlEmail in teamLeads)
                {
                    var tlLog = new NotificationLog
                    {
                        TicketId = integrationEvent.TicketId,
                        RecipientEmail = tlEmail,
                        ErrorMessage = null,
                        IsFailedDelivery = false,
                        SentAt = DateTime.UtcNow,
                        TemplateId = template.Id
                    };
                    await _repository.AddLogAsync(tlLog, cancellationToken);
                }
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged New Ticket Created notification successfully for Ticket {TicketNumber}", integrationEvent.TicketNumber);
    }
}

public sealed class TicketAssignedNotificationHandler : IIntegrationEventHandler<TicketAssignedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<TicketAssignedNotificationHandler> _logger;

    public TicketAssignedNotificationHandler(INotificationRepository repository, ILogger<TicketAssignedNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(TicketAssignedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TicketAssignedNotificationHandler for Ticket {TicketId}", integrationEvent.TicketId);

        var template = await _repository.GetTemplateByCodeAsync("TICKET_ASSIGNED_AGENT", cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email template TICKET_ASSIGNED_AGENT not found.");
            return;
        }

        var subject = template.Subject?
            .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
            .Replace("{{ticket.priority}}", "Medium")
            .Replace("{{ticket.company.name}}", "Adrenalin HRMS") ?? string.Empty;

        var body = template.BodyHtml?
            .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
            .Replace("{{ticket.priority}}", "Medium")
            .Replace("{{ticket.agent.name}}", "Agent")
            .Replace("{{ticket.url}}", $"http://localhost:4200/agent/tickets") ?? string.Empty;

        var assigneeEmail = await _repository.ResolveEmailAsync(integrationEvent.AssigneeId, cancellationToken) ?? "agent@adrenalin.com";

        var log = new NotificationLog
        {
            TicketId = integrationEvent.TicketId,
            RecipientEmail = assigneeEmail,
            ErrorMessage = null,
            IsFailedDelivery = false,
            SentAt = DateTime.UtcNow,
            TemplateId = template.Id
        };

        await _repository.AddLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged Ticket Assigned notification successfully for Ticket {TicketNumber}", integrationEvent.TicketNumber);
    }
}

public sealed class TicketResolvedNotificationHandler : IIntegrationEventHandler<TicketResolvedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<TicketResolvedNotificationHandler> _logger;

    public TicketResolvedNotificationHandler(INotificationRepository repository, ILogger<TicketResolvedNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(TicketResolvedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TicketResolvedNotificationHandler for Ticket {TicketId}", integrationEvent.TicketId);

        var template = await _repository.GetTemplateByCodeAsync("TICKET_RESOLVED_REQUESTER", cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email template TICKET_RESOLVED_REQUESTER not found.");
            return;
        }

        var subject = template.Subject?
            .Replace("{{ticket.subject}}", integrationEvent.TicketNumber) ?? string.Empty;

        var body = template.BodyHtml?
            .Replace("{{ticket.url}}", $"http://localhost:4200/customer-portal/ticket/{integrationEvent.TicketId}")
            .Replace("{{ticket.status}}", "Resolved")
            .Replace("{{helpdesk_name}}", "Adrenalin Helpdesk") ?? string.Empty;

        var reporterEmail = await _repository.GetTicketReporterEmailAsync(integrationEvent.TicketId, cancellationToken) ?? "customer@company.com";

        var log = new NotificationLog
        {
            TicketId = integrationEvent.TicketId,
            RecipientEmail = reporterEmail,
            ErrorMessage = null,
            IsFailedDelivery = false,
            SentAt = DateTime.UtcNow,
            TemplateId = template.Id
        };

        await _repository.AddLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged Ticket Resolved notification successfully for Ticket {TicketNumber}", integrationEvent.TicketNumber);
    }
}

public sealed class TicketClosedNotificationHandler : IIntegrationEventHandler<TicketClosedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<TicketClosedNotificationHandler> _logger;

    public TicketClosedNotificationHandler(INotificationRepository repository, ILogger<TicketClosedNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(TicketClosedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TicketClosedNotificationHandler for Ticket {TicketId}", integrationEvent.TicketId);

        var template = await _repository.GetTemplateByCodeAsync("TICKET_CLOSED_REQUESTER", cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email template TICKET_CLOSED_REQUESTER not found.");
            return;
        }

        var subject = template.Subject?
            .Replace("{{ticket.subject}}", integrationEvent.TicketNumber) ?? string.Empty;

        var body = template.BodyHtml?
            .Replace("{{ticket.id}}", integrationEvent.TicketNumber)
            .Replace("{{ticket.url}}", $"http://localhost:4200/customer-portal/ticket/{integrationEvent.TicketId}") ?? string.Empty;

        var reporterEmail = await _repository.GetTicketReporterEmailAsync(integrationEvent.TicketId, cancellationToken) ?? "customer@company.com";

        var log = new NotificationLog
        {
            TicketId = integrationEvent.TicketId,
            RecipientEmail = reporterEmail,
            ErrorMessage = null,
            IsFailedDelivery = false,
            SentAt = DateTime.UtcNow,
            TemplateId = template.Id
        };

        await _repository.AddLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged Ticket Closed notification successfully for Ticket {TicketNumber}", integrationEvent.TicketNumber);
    }
}

public sealed class TicketCommentAddedNotificationHandler : IIntegrationEventHandler<TicketCommentAddedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<TicketCommentAddedNotificationHandler> _logger;

    public TicketCommentAddedNotificationHandler(INotificationRepository repository, ILogger<TicketCommentAddedNotificationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(TicketCommentAddedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TicketCommentAddedNotificationHandler for Ticket {TicketId}", integrationEvent.TicketId);

        string templateCode = "AGENT_ADDED_COMMENT";
        if (integrationEvent.ContactId.HasValue)
        {
            templateCode = "REQUESTER_REPLIED";
        }
        else if (integrationEvent.Visibility == "Internal")
        {
            templateCode = "NOTE_ADDED_TO_TICKET";
        }

        var template = await _repository.GetTemplateByCodeAsync(templateCode, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email template {TemplateCode} not found.", templateCode);
            return;
        }

        var subject = template.Subject?
            .Replace("{{ticket.id}}", integrationEvent.TicketId.ToString())
            .Replace("{{ticket.subject}}", "Ticket Update") ?? string.Empty;

        var body = template.BodyHtml?
            .Replace("{{ticket.id}}", integrationEvent.TicketId.ToString())
            .Replace("{{comment.body}}", integrationEvent.Body)
            .Replace("{{ticket.url}}", $"http://localhost:4200/customer-portal/ticket/{integrationEvent.TicketId}") ?? string.Empty;

        string recipientEmail;
        if (integrationEvent.ContactId.HasValue)
        {
            recipientEmail = await _repository.GetTicketAssigneeEmailAsync(integrationEvent.TicketId, cancellationToken) ?? "agent@adrenalin.com";
        }
        else
        {
            recipientEmail = await _repository.GetTicketReporterEmailAsync(integrationEvent.TicketId, cancellationToken) ?? "customer@company.com";
        }

        var log = new NotificationLog
        {
            TicketId = integrationEvent.TicketId,
            RecipientEmail = recipientEmail,
            ErrorMessage = null,
            IsFailedDelivery = false,
            SentAt = DateTime.UtcNow,
            TemplateId = template.Id
        };

        await _repository.AddLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logged Comment Added notification successfully for Ticket {TicketId}", integrationEvent.TicketId);
    }
}
