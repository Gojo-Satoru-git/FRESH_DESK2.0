using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Application.Commands.Comments;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Application.Services;
using Adrenalin.SharedKernel.Contracts;

using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Adrenalin.Modules.Ticketing.Application.IntegrationEvents;

public sealed class ProcessInboundEmailIntegrationEventHandler : IIntegrationEventHandler<EmailReceivedIntegrationEvent>
{
    private readonly IProcessedEmailLogRepository _processedEmailLogRepository;
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IEmailAliasRoutingRepository _emailAliasRoutingRepository;
    private readonly IEmailThreadDetectionService _emailThreadDetectionService;
    private readonly IEmailWatcherExtractionService _emailWatcherExtractionService;
    private readonly IIntegrationEventLogRepository _eventLogRepository;
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<ProcessInboundEmailIntegrationEventHandler> _logger;
    private readonly ITicketRepository _ticketRepository;

    public ProcessInboundEmailIntegrationEventHandler(
        IProcessedEmailLogRepository processedEmailLogRepository,
        IEmailMessageRepository emailMessageRepository,
        IEmailAliasRoutingRepository emailAliasRoutingRepository,
        IEmailThreadDetectionService emailThreadDetectionService,
        IEmailWatcherExtractionService emailWatcherExtractionService,
        IIntegrationEventLogRepository eventLogRepository,
        IDispatcher dispatcher,
        ILogger<ProcessInboundEmailIntegrationEventHandler> logger,
        ITicketRepository ticketRepository)
    {
        _processedEmailLogRepository = processedEmailLogRepository;
        _emailMessageRepository = emailMessageRepository;
        _emailAliasRoutingRepository = emailAliasRoutingRepository;
        _emailThreadDetectionService = emailThreadDetectionService;
        _emailWatcherExtractionService = emailWatcherExtractionService;
        _eventLogRepository = eventLogRepository;
        _dispatcher = dispatcher;
        _logger = logger;
        _ticketRepository = ticketRepository;
    }

    public async Task HandleAsync(EmailReceivedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        EmailMetrics.EmailsReceived.Add(1);

        // Idempotency: event-level dedup
        if (await _eventLogRepository.HasEventBeenProcessedAsync(integrationEvent.EventId, cancellationToken))
            return;

        _logger.LogInformation("Processing incoming email {MessageId}", integrationEvent.MessageId);

        // Idempotency: email-level dedup
        if (await _processedEmailLogRepository.HasBeenProcessedAsync(integrationEvent.InternetMessageId, cancellationToken))
        {
            EmailMetrics.EmailsDeduplicated.Add(1);
            _logger.LogWarning("Email {InternetMessageId} has already been processed.", integrationEvent.InternetMessageId);
            return;
        }

        var routingAlias = await _emailAliasRoutingRepository.GetBestMatchAsync(integrationEvent.ToEmail, cancellationToken);
        var companyId = routingAlias?.CompanyId;

        // Resolve contact
        var resolveContactCommand = new ResolveEmailContactContractCommand(
            integrationEvent.FromEmail,
            integrationEvent.FromName,
            null
        );

        var resolveResult = await _dispatcher.Send(resolveContactCommand, cancellationToken);
        
        if (resolveResult.IsSuccess && resolveResult.Value != null && resolveResult.Value.AutoCreated)
        {
            EmailMetrics.AutoContactsCreated.Add(1);
        }

        Guid finalCompanyId = companyId ?? resolveResult.Value?.CompanyId ?? Guid.Empty;
        Guid? finalContactId = resolveResult.Value?.ContactId;

        if (finalCompanyId == Guid.Empty)
        {
            EmailMetrics.EmailsIgnored.Add(1);
            _logger.LogWarning("Could not resolve Company for email {MessageId}. Skipping processing.", integrationEvent.MessageId);
            await LogProcessedAsync(integrationEvent, EmailProcessingState.Failed, "UnresolvedCompany", cancellationToken);
            return;
        }

        // Sanitize HTML body
        var bodyHtml = SanitizeHtml(integrationEvent.BodyHtml, integrationEvent.MessageId);

        // Store Email Message
        var emailMessage = new EmailMessage
        {
            Provider = integrationEvent.Provider,
            MessageId = integrationEvent.MessageId,
            InternetMessageId = integrationEvent.InternetMessageId,
            ThreadId = integrationEvent.ThreadId,
            InReplyTo = integrationEvent.InReplyTo,
            References = integrationEvent.References,
            SenderEmail = integrationEvent.FromEmail,
            SenderName = integrationEvent.FromName,
            RecipientEmail = integrationEvent.ToEmail,
            Subject = integrationEvent.Subject,
            BodyHtml = bodyHtml,
            BodyText = integrationEvent.BodyText,
            ReceivedAt = integrationEvent.ReceivedAt.ToUniversalTime(),
            ProcessingState = EmailProcessingState.Processing
        };

        // Validate and filter attachments
        var validAttachments = ValidateAttachments(integrationEvent.Attachments);

        foreach (var att in validAttachments)
        {
            emailMessage.AddAttachment(new EmailAttachment
            {
                FileName = att.FileName,
                ContentType = att.ContentType,
                Size = att.Size,
                StoragePath = null // Will be assigned during physical storage
            });
        }

        await _emailMessageRepository.AddAsync(emailMessage, cancellationToken);

        // Thread Detection — TENANT-SAFE (Fix C-1)
        Guid? senderUserId = resolveResult.Value?.UserId;
        Guid? existingTicketId = await _emailThreadDetectionService.DetectThreadAsync(integrationEvent, finalCompanyId, senderUserId, cancellationToken);

        // Watcher Extraction
        string systemEmail = routingAlias?.EmailAddress ?? "system@adrenalin.com";
        List<Guid> watcherUserIds = new();
        try
        {
            watcherUserIds = await _emailWatcherExtractionService.ExtractWatchersAsync(
                integrationEvent.ToEmail,
                integrationEvent.CcEmails,
                integrationEvent.FromEmail,
                systemEmail,
                finalCompanyId,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Watcher extraction failed.");
            watcherUserIds = new List<Guid>();
        }

        if (existingTicketId.HasValue)
        {
            var addReplyCommand = new AddEmailReplyCommand(
                existingTicketId.Value,
                emailMessage.Id,
                finalContactId,
                null,
                integrationEvent.BodyText ?? "",
                integrationEvent.BodyHtml,
                watcherUserIds,
                false,
                validAttachments
            );

            await _dispatcher.Send(addReplyCommand, cancellationToken);
            EmailMetrics.RepliesAdded.Add(1);
            _logger.LogInformation("Added reply to existing ticket {TicketId}", existingTicketId.Value);
        }
        else
        {
            var (moduleId, _, _) = await _ticketRepository.ResolveOrCreateModuleAsync(TicketType.Incident.ToString(), cancellationToken);

            var createTicketCommand = new CreateTicketFromEmailCommand(
                finalCompanyId,
                finalContactId,
                moduleId,
                emailMessage.Id,
                integrationEvent.Subject,
                integrationEvent.BodyText ?? "",
                integrationEvent.BodyHtml,
                watcherUserIds,
                null,
                validAttachments
            );

            var createResult = await _dispatcher.Send(createTicketCommand, cancellationToken);
            
            if (!createResult.IsSuccess)
            {
                EmailMetrics.EmailsFailed.Add(1);
                _logger.LogError("Failed to create ticket: {Error}", createResult.Error);
                await LogProcessedAsync(integrationEvent, EmailProcessingState.Failed, createResult.Error ?? "Unknown Error", cancellationToken);
                return;
            }

            EmailMetrics.TicketsCreated.Add(1);
            _logger.LogInformation("Created new ticket {TicketId}", createResult.Value);
        }

        // Persist ProcessedEmailLog with concurrency guard (Fix H-2)
        try
        {
            await LogProcessedAsync(integrationEvent, EmailProcessingState.Processed, null, cancellationToken);
        }
        catch (Exception ex) when (IsDuplicateKeyException(ex))
        {
            // A concurrent handler already inserted the ProcessedEmailLog for this InternetMessageId.
            // The unique index on InternetMessageId prevented the duplicate. This is safe to swallow
            // because the other handler already processed this email successfully.
            _logger.LogWarning(
                "Concurrent duplicate detected for {InternetMessageId}. Another handler already processed this email.",
                integrationEvent.InternetMessageId);
            EmailMetrics.EmailsDeduplicated.Add(1);
            return;
        }

        await _eventLogRepository.MarkEventAsProcessedAsync(new Adrenalin.SharedKernel.Entities.IntegrationEventLog
        {
            Id = Guid.NewGuid(),
            EventId = integrationEvent.EventId,
            EventType = integrationEvent.GetType().Name,
            ProcessedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await _eventLogRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task LogProcessedAsync(EmailReceivedIntegrationEvent integrationEvent, EmailProcessingState status, string? errorReason, CancellationToken cancellationToken)
    {
        var log = new ProcessedEmailLog
        {
            InternetMessageId = integrationEvent.InternetMessageId,
            Provider = integrationEvent.Provider,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = status,
            FailureReason = errorReason
        };
        await _processedEmailLogRepository.AddLogAsync(log, cancellationToken);
    }

    private string? SanitizeHtml(string? bodyHtml, string messageId)
    {
        if (string.IsNullOrEmpty(bodyHtml)) return bodyHtml;

        if (Regex.IsMatch(bodyHtml, @"(?i)(<script|on\w+=|javascript:|<iframe|<embed|<object)"))
        {
            _logger.LogWarning("Potential XSS detected in email {MessageId}. Sanitizing body.", messageId);
            // Strip script tags and their content
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)<script[\s\S]*?</script>", "", RegexOptions.Compiled);
            // Strip dangerous elements
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)<(iframe|embed|object)[^>]*>.*?</(iframe|embed|object)>", "", RegexOptions.Compiled);
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)<(iframe|embed|object)[^>]*/?>", "", RegexOptions.Compiled);
            // Strip all on* event handler attributes
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)\s+on\w+\s*=\s*""[^""]*""", "", RegexOptions.Compiled);
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)\s+on\w+\s*=\s*'[^']*'", "", RegexOptions.Compiled);
            // Strip javascript: protocol in href/src
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)(href|src)\s*=\s*""javascript:[^""]*""", "$1=\"\"", RegexOptions.Compiled);
            bodyHtml = Regex.Replace(bodyHtml, @"(?i)(href|src)\s*=\s*'javascript:[^']*'", "$1=''", RegexOptions.Compiled);
        }
        return bodyHtml;
    }

    private List<EmailAttachmentDto> ValidateAttachments(IReadOnlyList<EmailAttachmentDto>? attachments)
    {
        var validAttachments = new List<EmailAttachmentDto>();
        if (attachments == null || !attachments.Any()) return validAttachments;

        // Deduplicate using combination of FileName, Size, ContentType and ContentId
        var uniqueAttachments = attachments
            .GroupBy(a => new { a.FileName, a.Size, a.ContentType, a.ContentId })
            .Select(g => g.First())
            .ToList();

        var blockedExtensions = new[] { ".exe", ".js", ".bat", ".cmd", ".scr", ".ps1", ".vbs", ".msi", ".com", ".pif", ".jar", ".wsf", ".sh", ".bash", ".psm1", ".dll", ".sys" };

        foreach (var att in uniqueAttachments)
        {
            if (att.Size > 25 * 1024 * 1024)
            {
                _logger.LogWarning("Attachment {FileName} rejected: Exceeds 25MB.", att.FileName);
                EmailMetrics.AttachmentsRejected.Add(1);
                continue;
            }

            var fileNameLower = att.FileName.ToLowerInvariant();

            // Check all extensions in filename (catches double extensions like .pdf.exe)
            var parts = fileNameLower.Split('.');
            if (parts.Length >= 2)
            {
                bool blocked = false;
                for (int i = 1; i < parts.Length; i++)
                {
                    if (blockedExtensions.Contains("." + parts[i]))
                    {
                        _logger.LogWarning("Attachment {FileName} rejected: Contains blocked extension .{Ext}.", att.FileName, parts[i]);
                        EmailMetrics.AttachmentsRejected.Add(1);
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;
            }

            validAttachments.Add(att);
        }

        return validAttachments;
    }

    private static bool IsDuplicateKeyException(Exception ex)
    {
        // Check if the exception or inner exception is a DbUpdateException (by name) and has duplicate key violation
        if (ex.GetType().Name.Contains("DbUpdateException") || 
            (ex.InnerException != null && ex.InnerException.GetType().Name.Contains("DbUpdateException")))
        {
            return ex.InnerException?.Message?.Contains("23505") == true
                || ex.InnerException?.Message?.Contains("duplicate key") == true;
        }
        return false;
    }
}
