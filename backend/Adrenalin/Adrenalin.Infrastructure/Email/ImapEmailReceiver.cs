using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Infrastructure.Email;

public sealed class ImapEmailReceiver : IEmailReceive
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImapEmailReceiver> _logger;
    private int _simulationStep = 0;

    public ImapEmailReceiver(IConfiguration configuration, ILogger<ImapEmailReceiver> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InboundEmail>> ReceiveEmailsAsync(CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection("EmailInbox");
        var enabled = section.GetValue<bool>("Enabled", true);
        if (!enabled)
        {
            return Array.Empty<InboundEmail>();
        }

        var host = section.GetValue<string>("Host");
        var username = section.GetValue<string>("Username");
        var password = section.GetValue<string>("Password");
        var simulate = section.GetValue<bool>("Simulate", false);

        if (simulate)
        {
            return GetSimulatedEmails();
        }

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            _logger.LogError("IMAP Host or Username is not configured in appsettings.json. Real email ingestion failed.");
            return Array.Empty<InboundEmail>();
        }

        // Real IMAP parsing using simple TCP/SSL IMAP Client commands
        try
        {
            var port = section.GetValue<int>("Port", 993);
            var useSsl = section.GetValue<bool>("UseSsl", true);

            using var client = new TcpClient();
            await client.ConnectAsync(host!, port, cancellationToken);

            Stream stream = client.GetStream();
            if (useSsl)
            {
                var sslStream = new SslStream(stream, false, (sender, certificate, chain, errors) => true);
                await sslStream.AuthenticateAsClientAsync(host!);
                stream = sslStream;
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Read welcome greeting
            await reader.ReadLineAsync(cancellationToken);

            // 1. LOGIN
            await writer.WriteLineAsync($"A1 LOGIN {username} {password}");
            var loginResponse = await ReadTagResponseAsync(reader, "A1", cancellationToken);
            if (!loginResponse.StartsWith("A1 OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("IMAP Login failed: {Response}", loginResponse);
                return Array.Empty<InboundEmail>();
            }

            // 2. SELECT INBOX
            await writer.WriteLineAsync("A2 SELECT INBOX");
            var selectResponse = await ReadTagResponseAsync(reader, "A2", cancellationToken);
            if (!selectResponse.StartsWith("A2 OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("IMAP Select failed: {Response}", selectResponse);
                return Array.Empty<InboundEmail>();
            }

            // 3. SEARCH UNSEEN
            await writer.WriteLineAsync("A3 SEARCH UNSEEN");
            var searchResponse = await ReadSearchResponseAsync(reader, cancellationToken);
            
            var emails = new List<InboundEmail>();
            foreach (var msgId in searchResponse)
            {
                // 4. FETCH HEADER AND BODY
                await writer.WriteLineAsync($"A4 FETCH {msgId} (BODY[HEADER.FIELDS (FROM SUBJECT)] BODY[TEXT])");
                var fetchResponse = await ReadFetchContentAsync(reader, cancellationToken);
                
                // Parse headers and body
                var inbound = ParseFetchResponse(fetchResponse);
                if (inbound != null)
                {
                    emails.Add(inbound);
                    // 5. STORE +FLAGS (\Seen) to mark as read
                    await writer.WriteLineAsync($"A5 STORE {msgId} +FLAGS (\\Seen)");
                    await ReadTagResponseAsync(reader, "A5", cancellationToken);
                }
            }

            // 6. LOGOUT
            await writer.WriteLineAsync("A6 LOGOUT");
            await ReadTagResponseAsync(reader, "A6", cancellationToken);

            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive emails via IMAP.");
            if (simulate)
            {
                _logger.LogInformation("Falling back to simulation mode.");
                return GetSimulatedEmails();
            }
            throw;
        }
    }

    private static async Task<string> ReadTagResponseAsync(StreamReader reader, string tag, CancellationToken ct)
    {
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (line.StartsWith(tag, StringComparison.OrdinalIgnoreCase))
            {
                return line;
            }
        }
        return string.Empty;
    }

    private static async Task<List<int>> ReadSearchResponseAsync(StreamReader reader, CancellationToken ct)
    {
        var ids = new List<int>();
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (line.StartsWith("* SEARCH", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(' ');
                for (int i = 2; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i], out var id))
                    {
                        ids.Add(id);
                    }
                }
            }
            if (line.StartsWith("A3 OK", StringComparison.OrdinalIgnoreCase) || line.StartsWith("A3 BAD", StringComparison.OrdinalIgnoreCase) || line.StartsWith("A3 NO", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }
        return ids;
    }

    private static async Task<string> ReadFetchContentAsync(StreamReader reader, CancellationToken ct)
    {
        var sb = new StringBuilder();
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            sb.AppendLine(line);
            if (line.StartsWith("A4 OK", StringComparison.OrdinalIgnoreCase) || line.StartsWith("A4 BAD", StringComparison.OrdinalIgnoreCase) || line.StartsWith("A4 NO", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }
        return sb.ToString();
    }

    private static InboundEmail? ParseFetchResponse(string response)
    {
        var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        string from = string.Empty;
        string subject = string.Empty;
        var bodyLines = new List<string>();
        bool inBody = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("From:", StringComparison.OrdinalIgnoreCase))
            {
                from = line[5..].Trim();
            }
            else if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
            {
                subject = line[8..].Trim();
            }
            else if (string.IsNullOrWhiteSpace(line) && !inBody)
            {
                inBody = true;
            }
            else if (inBody && !line.StartsWith("A4 OK", StringComparison.OrdinalIgnoreCase) && !line.Contains("FETCH"))
            {
                bodyLines.Add(rawLine);
            }
        }

        if (string.IsNullOrWhiteSpace(from))
            return null;

        // Parse SenderEmail and SenderName from From header: "Name <email@domain.com>" or "email@domain.com"
        string email = from;
        string name = from;
        if (from.Contains("<") && from.Contains(">"))
        {
            var open = from.IndexOf('<');
            var close = from.IndexOf('>');
            name = from[..open].Trim().Replace("\"", "");
            email = from[(open + 1)..close].Trim();
        }

        return new InboundEmail(
            SenderEmail: email,
            SenderName: string.IsNullOrWhiteSpace(name) ? email : name,
            Subject: subject,
            Body: string.Join("\n", bodyLines)
        );
    }

    private IReadOnlyList<InboundEmail> GetSimulatedEmails()
    {
        var emails = new List<InboundEmail>();

        // Return a simulated email depending on the step
        switch (_simulationStep % 4)
        {
            case 0:
                _logger.LogInformation("IMAP Simulation: Simulating email from existing customer 'rahul@gmail.com'");
                emails.Add(new InboundEmail(
                    SenderEmail: "rahul@gmail.com",
                    SenderName: "Rahul O S",
                    Subject: "Re: Need support with login configuration",
                    Body: "Hi support team,\n\nI am facing issues when configuring the login. Can someone assist me?\n\nThanks,\nRahul"
                ));
                break;

            case 1:
                _logger.LogInformation("IMAP Simulation: Simulating email from new contact 'new.employee@abc.org' (domain abc.org matches existing customer)");
                emails.Add(new InboundEmail(
                    SenderEmail: "new.employee@abc.org",
                    SenderName: "New Employee",
                    Subject: "Access request to ticketing dashboard",
                    Body: "Please grant me access to the support ticketing dashboard. I belong to the ABC Org customer account."
                ));
                break;

            case 2:
                _logger.LogInformation("IMAP Simulation: Simulating email from unknown company domain 'service@spacex.com'");
                emails.Add(new InboundEmail(
                    SenderEmail: "service@spacex.com",
                    SenderName: "SpaceX Service Portal",
                    Subject: "Critical: Software Enhancement requests",
                    Body: "Greetings,\n\nWe need to schedule a software enhancement deployment request for our telemetry module. Please create a ticket."
                ));
                break;
            default:
                _logger.LogInformation("IMAP Simulation: No new emails found.");
                break;
        }

        _simulationStep++;
        return emails;
    }
}
