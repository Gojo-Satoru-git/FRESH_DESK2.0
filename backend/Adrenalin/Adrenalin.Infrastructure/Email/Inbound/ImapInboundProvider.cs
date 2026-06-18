using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Adrenalin.EventBus.Events;

namespace Adrenalin.Infrastructure.Email.Inbound;

public sealed class ImapInboundProvider : IInboundEmailProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImapInboundProvider> _logger;

    public string ProviderName => "IMAP";

    public ImapInboundProvider(IConfiguration configuration, ILogger<ImapInboundProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EmailReceivedIntegrationEvent>> ReceiveEmailsAsync(CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection("Email:EmailInbox");
        var enabled = section.GetValue<bool>("Enabled", true);
        if (!enabled)
            return Array.Empty<EmailReceivedIntegrationEvent>();

        var host = section.GetValue<string>("Host");
        var port = section.GetValue<int>("Port", 993);
        var useSsl = section.GetValue<bool>("UseSsl", true);
        var username = section.GetValue<string>("Username") ?? string.Empty;
        var password = section.GetValue<string>("Password") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("IMAP settings missing in configuration.");
            return Array.Empty<EmailReceivedIntegrationEvent>();
        }

        var events = new List<EmailReceivedIntegrationEvent>();

        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(host, port, useSsl, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            var uids = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);

            foreach (var uid in uids)
            {
                try
                {
                    var message = await inbox.GetMessageAsync(uid, cancellationToken);
                    
                    var attachments = new List<EmailAttachmentDto>();
                    foreach (var attachment in message.Attachments.OfType<MimePart>())
                    {
                        if (attachment.Content == null) continue;

                        using var ms = new MemoryStream();
                        await attachment.Content.DecodeToAsync(ms, cancellationToken);
                        attachments.Add(new EmailAttachmentDto(
                            attachment.FileName ?? "unnamed",
                            attachment.ContentType.MimeType,
                            ms.Length,
                            attachment.ContentId,
                            ms.ToArray()
                        ));
                    }

                    var headers = message.Headers.GroupBy(h => h.Field).ToDictionary(g => g.Key, g => g.First().Value);
                    
                    var sender = message.From.Mailboxes.FirstOrDefault();
                    var fromEmail = sender?.Address ?? "unknown@adrenalin.com";
                    var fromName = sender?.Name ?? fromEmail;

                    var ev = new EmailReceivedIntegrationEvent(
                        Guid.NewGuid(),
                        ProviderName,
                        message.MessageId ?? Guid.NewGuid().ToString(),
                        message.MessageId ?? Guid.NewGuid().ToString(), // InternetMessageId
                        message.Headers["Thread-Index"], // ThreadId heuristic
                        message.InReplyTo,
                        message.References?.FirstOrDefault(), // simplified string
                        message.Subject ?? "No Subject",
                        message.TextBody,
                        message.HtmlBody,
                        fromEmail,
                        fromName,
                        message.To?.Mailboxes?.FirstOrDefault()?.Address ?? string.Empty,
                        message.Cc?.Mailboxes?.Select(m => m.Address).ToList() ?? new List<string>(),
                        message.Date,
                        headers,
                        attachments
                    );

                    events.Add(ev);
                    await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse message UID {Uid}", uid);
                }
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IMAP processing failed.");
        }

        return events;
    }
}
