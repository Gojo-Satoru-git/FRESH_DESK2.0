using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/v1/admin/email-ingestion")]
// [Authorize(Roles = "SuperAdmin")] // Assuming standard auth
public class EmailIngestionController : ControllerBase
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EmailIngestionController> _logger;

    public EmailIngestionController(IEventBus eventBus, ILogger<EmailIngestionController> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    [HttpPost("trigger-test")]
    public async Task<IActionResult> TriggerTest([FromBody] TriggerTestRequest request)
    {
        _logger.LogInformation("Manually triggering email ingestion test for {Email}", request.FromAddress);

        var evt = new EmailReceivedIntegrationEvent(
            EventId: Guid.NewGuid(),
            Provider: request.Provider ?? "TestProvider",
            MessageId: request.MessageId ?? Guid.NewGuid().ToString(),
            InternetMessageId: $"<{Guid.NewGuid()}@test.local>",
            ThreadId: null,
            InReplyTo: request.InReplyTo,
            References: request.References,
            Subject: request.Subject,
            BodyText: request.PlainTextBody,
            BodyHtml: request.HtmlBody,
            FromEmail: request.FromAddress,
            FromName: request.FromName,
            ToEmail: request.ToAddress ?? "support@company.com",
            CcEmails: new System.Collections.Generic.List<string>(),
            ReceivedAt: DateTimeOffset.UtcNow,
            Headers: new System.Collections.Generic.Dictionary<string, string>(),
            Attachments: new System.Collections.Generic.List<EmailAttachmentDto>()
        );

        await _eventBus.PublishAsync(evt);

        return Ok(new { Message = "EmailReceivedIntegrationEvent published successfully", EventId = evt.EventId });
    }
}

public class TriggerTestRequest
{
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string? ToAddress { get; set; }
    public string Subject { get; set; } = null!;
    public string PlainTextBody { get; set; } = null!;
    public string? HtmlBody { get; set; }
    public string? MessageId { get; set; }
    public string? Provider { get; set; }
    public string? InReplyTo { get; set; }
    public string? References { get; set; }
}
