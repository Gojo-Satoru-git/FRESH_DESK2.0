using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Persistence.Outbox;

public sealed class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;

    public OutboxProcessorBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            // Polling interval
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();
        var rabbitMqPublisher = scope.ServiceProvider.GetRequiredService<RabbitMQEventBus>();

        // Fetch up to 50 unprocessed or failed messages (with retry limit)
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOn)
            .Take(50)
            .ToListAsync(stoppingToken);

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                // Deserialize to a dynamic object to pass to RabbitMQEventBus
                // Since RabbitMQEventBus uses JsonSerializer.Serialize(integrationEvent), 
                // we can just pass a raw object or recreate the specific type.
                // However, RabbitMQEventBus uses Type.Name for routing key.
                // We will need to modify RabbitMQEventBus to allow publishing raw JSON with eventName.
                
                // Let's call a new method on RabbitMQEventBus or just pass an object.
                // Wait, RabbitMQEventBus.PublishAsync<T> where T: class.
                // Since payload is already serialized, we can add PublishRawAsync to RabbitMQEventBus.
                
                await rabbitMqPublisher.PublishRawAsync(message.EventType, message.Payload, stoppingToken);

                message.ProcessedOn = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                message.ErrorMessage = ex.Message;
                message.RetryCount++;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
