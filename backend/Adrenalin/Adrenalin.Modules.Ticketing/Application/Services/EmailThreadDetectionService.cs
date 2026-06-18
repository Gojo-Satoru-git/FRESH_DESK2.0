using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public class EmailThreadDetectionService : IEmailThreadDetectionService
{
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<EmailThreadDetectionService> _logger;

    public EmailThreadDetectionService(
        IEmailMessageRepository emailMessageRepository,
        ITicketRepository ticketRepository,
        ILogger<EmailThreadDetectionService> logger)
    {
        _emailMessageRepository = emailMessageRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<Guid?> DetectThreadAsync(EmailReceivedIntegrationEvent integrationEvent, Guid companyId, Guid? senderUserId, CancellationToken cancellationToken = default)
    {
        bool isInternalAgent = senderUserId.HasValue && await _ticketRepository.IsUserInternalAsync(senderUserId.Value, cancellationToken);

        // 1. Ticket Number from Subject (highest priority)
        if (!string.IsNullOrWhiteSpace(integrationEvent.Subject))
        {
            var subjectStripped = System.Text.RegularExpressions.Regex.Replace(integrationEvent.Subject, @"^(?i)(re|fwd|fw|aw):\s*", "").Trim();
            var match = System.Text.RegularExpressions.Regex.Match(subjectStripped, @"TKT-\d{4}-\d{6}");
            if (match.Success)
            {
                var ticketNumber = match.Value;
                var ticket = await _ticketRepository.GetByTicketNumberAsync(ticketNumber, cancellationToken);
                if (ticket != null)
                {
                    if (isInternalAgent || ticket.CompanyId == companyId)
                    {
                        return ticket.Id;
                    }
                    _logger.LogWarning(
                        "Tenant boundary violation blocked: Email from Company {SenderCompany} tried to match ticket {TicketNumber} belonging to Company {TicketCompany}",
                        companyId, ticketNumber, ticket.CompanyId);
                }
            }
        }

        // 2. Check InReplyTo
        if (!string.IsNullOrWhiteSpace(integrationEvent.InReplyTo))
        {
            var ticketId = await TryResolveTicketFromMessageIdAsync(integrationEvent.InReplyTo.Trim(), companyId, isInternalAgent, cancellationToken);
            if (ticketId.HasValue) return ticketId;
        }

        // 3. Check References (space-separated Message-IDs, newest to oldest)
        if (!string.IsNullOrWhiteSpace(integrationEvent.References))
        {
            var refs = integrationEvent.References
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Reverse();

            var candidates = new System.Collections.Generic.List<(Guid TicketId, DateTimeOffset ReceivedAt)>();

            foreach (var reference in refs)
            {
                var parentMsg = await _emailMessageRepository.GetByInternetMessageIdAsync(reference, cancellationToken);
                if (parentMsg != null && parentMsg.TicketId.HasValue)
                {
                    var parentTicket = await _ticketRepository.GetByIdAsync(parentMsg.TicketId.Value, cancellationToken);
                    if (parentTicket != null && (isInternalAgent || parentTicket.CompanyId == companyId))
                    {
                        candidates.Add((parentTicket.Id, parentMsg.ReceivedAt));
                    }
                }
            }

            if (candidates.Any())
            {
                return candidates.OrderByDescending(c => c.ReceivedAt).First().TicketId;
            }
        }

        // 4. Check InternetMessageId fallback
        if (!string.IsNullOrWhiteSpace(integrationEvent.InternetMessageId))
        {
            var ticketId = await TryResolveTicketFromMessageIdAsync(integrationEvent.InternetMessageId.Trim(), companyId, isInternalAgent, cancellationToken);
            if (ticketId.HasValue) return ticketId;
        }

        // 5. Check ThreadId (provider-specific thread correlation)
        if (!string.IsNullOrWhiteSpace(integrationEvent.ThreadId))
        {
            var msgs = await _emailMessageRepository.GetByThreadIdAsync(integrationEvent.ThreadId.Trim(), cancellationToken);
            if (msgs != null && msgs.Any())
            {
                foreach (var msg in msgs.OrderByDescending(m => m.ReceivedAt))
                {
                    if (msg.TicketId.HasValue)
                    {
                        var ticket = await _ticketRepository.GetByIdAsync(msg.TicketId.Value, cancellationToken);
                        if (ticket != null && (isInternalAgent || ticket.CompanyId == companyId))
                        {
                            return ticket.Id;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves a TicketId from an InternetMessageId, validating tenant boundary.
    /// </summary>
    private async Task<Guid?> TryResolveTicketFromMessageIdAsync(string internetMessageId, Guid companyId, bool isInternalAgent, CancellationToken cancellationToken)
    {
        var parentMsg = await _emailMessageRepository.GetByInternetMessageIdAsync(internetMessageId, cancellationToken);
        if (parentMsg != null && parentMsg.TicketId.HasValue)
        {
            var parentTicket = await _ticketRepository.GetByIdAsync(parentMsg.TicketId.Value, cancellationToken);
            if (parentTicket != null && (isInternalAgent || parentTicket.CompanyId == companyId))
            {
                return parentTicket.Id;
            }
            if (parentTicket != null)
            {
                _logger.LogWarning(
                    "Tenant boundary violation blocked: Message {MessageId} maps to ticket in Company {TicketCompany}, but sender belongs to Company {SenderCompany}",
                    internetMessageId, parentTicket.CompanyId, companyId);
            }
        }
        return null;
    }
}
