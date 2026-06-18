using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Adrenalin.Infrastructure.Email.Inbound;
using Adrenalin.EventBus;

namespace Adrenalin.unify.API.BackgroundJobs;

public sealed class EmailIngestionBackgroundService : BackgroundService
{
    private readonly IEnumerable<IInboundEmailProvider> _providers;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailIngestionBackgroundService> _logger;

    public EmailIngestionBackgroundService(
        IEnumerable<IInboundEmailProvider> providers,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<EmailIngestionBackgroundService> logger)
    {
        _providers = providers;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var section = _configuration.GetSection("Email:EmailInbox");
        var enabled = section.GetValue<bool>("Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("Email Ingestion is disabled in configuration.");
            return;
        }

        var intervalSec = section.GetValue<int>("PollIntervalSeconds", 60);
        if (intervalSec < 5) intervalSec = 5;

        _logger.LogInformation("Email Ingestion Background Service started. Polling every {Interval} seconds.", intervalSec);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var provider in _providers)
                {
                    var emails = await provider.ReceiveEmailsAsync(stoppingToken);
                    if (emails.Count > 0)
                    {
                        _logger.LogInformation("Provider {Provider} received {Count} new emails.", provider.ProviderName, emails.Count);
                        using var scope = _serviceProvider.CreateScope();
                        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                        // Outbox requires db context save changes to commit events!
                        var dbContext = scope.ServiceProvider.GetRequiredService<Adrenalin.Persistence.Context.AdrenalinDbContext>();

                        foreach (var emailEvent in emails)
                        {
                            await eventBus.PublishAsync(emailEvent, stoppingToken);
                            _logger.LogInformation("Published EmailReceivedIntegrationEvent for MessageId {MessageId}", emailEvent.MessageId);
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation("Provider {Provider} found 0 new emails.", provider.ProviderName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email ingestion polling.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSec), stoppingToken);
        }
    }
}
