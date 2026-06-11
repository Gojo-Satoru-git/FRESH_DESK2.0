using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Adrenalin.Infrastructure.Email;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.unify.API.BackgroundJobs;

public sealed class EmailPollingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailReceive _emailReceiver;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailPollingJob> _logger;

    public EmailPollingJob(
        IServiceScopeFactory scopeFactory,
        IEmailReceive emailReceiver,
        IConfiguration configuration,
        ILogger<EmailPollingJob> logger)
    {
        _scopeFactory = scopeFactory;
        _emailReceiver = emailReceiver;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Polling Job background worker started.");

        var section = _configuration.GetSection("EmailInbox");
        var enabled = section.GetValue<bool>("Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("Email Polling Job is disabled in configuration.");
            return;
        }

        var intervalSec = section.GetValue<int>("PollIntervalSeconds", 60);
        if (intervalSec < 5) intervalSec = 5;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Email Polling Job checking for new emails...");
                var incomingEmails = await _emailReceiver.ReceiveEmailsAsync(stoppingToken);

                if (incomingEmails != null && incomingEmails.Count > 0)
                {
                    _logger.LogInformation("Found {Count} new email(s) to process.", incomingEmails.Count);

                    foreach (var email in incomingEmails)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

                        var title = EmailParser.CleanSubject(email.Subject);
                        var description = EmailParser.CleanBody(email.Body);
                        
                        _logger.LogInformation("Processing email ticket creation for sender {Sender} with subject '{Subject}'", email.SenderEmail, title);

                        var command = new CreateTicketCommand(
                            Title: title,
                            Description: description,
                            Priority: "Medium",
                            Type: "Incident",
                            
                            ActorId: null,
                            IsCustomer: false,
                            SenderEmail: email.SenderEmail
                        );

                        try
                        {
                            var ticketId = await dispatcher.Send(command, stoppingToken);
                            _logger.LogInformation("Successfully created ticket {TicketId} from email sent by {Sender}", ticketId, email.SenderEmail);
                        }
                        catch (Exception createEx)
                        {
                            _logger.LogError(createEx, "Failed to create ticket from email sent by {Sender}", email.SenderEmail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during email polling process execution.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSec), stoppingToken);
        }

        _logger.LogInformation("Email Polling Job background worker stopped.");
    }
}
