using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus.Events;

namespace Adrenalin.EventBus;

public sealed class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int retryCount = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning("RabbitMQ connection attempt {Retry} failed. Retrying in 5 seconds... Error: {Error}", retryCount, ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (stoppingToken.IsCancellationRequested || _channel == null)
            return;

        var exchangeName = "adrenalin.events";
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: stoppingToken);

        var queueName = "adrenalin.ticketing.queue";
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "",
            cancellationToken: stoppingToken);

        _logger.LogInformation("Successfully connected to RabbitMQ and bound queue {QueueName} to {ExchangeName}", queueName, exchangeName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("RabbitMQ message received: {EventName}", routingKey);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                await ProcessEventAsync(scope.ServiceProvider, routingKey, message, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventName}", routingKey);
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessEventAsync(IServiceProvider serviceProvider, string eventName, string message, CancellationToken ct)
    {
        switch (eventName)
        {
            case nameof(TicketCreatedIntegrationEvent):
                var createdEvent = JsonSerializer.Deserialize<TicketCreatedIntegrationEvent>(message);
                if (createdEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TicketCreatedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(createdEvent, ct);
                }
                break;

            case nameof(TicketAssignedIntegrationEvent):
                var assignedEvent = JsonSerializer.Deserialize<TicketAssignedIntegrationEvent>(message);
                if (assignedEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TicketAssignedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(assignedEvent, ct);
                }
                break;

            case nameof(TicketResolvedIntegrationEvent):
                var resolvedEvent = JsonSerializer.Deserialize<TicketResolvedIntegrationEvent>(message);
                if (resolvedEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TicketResolvedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(resolvedEvent, ct);
                }
                break;

            case nameof(TicketClosedIntegrationEvent):
                var closedEvent = JsonSerializer.Deserialize<TicketClosedIntegrationEvent>(message);
                if (closedEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TicketClosedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(closedEvent, ct);
                }
                break;

            case nameof(TicketCommentAddedIntegrationEvent):
                var commentEvent = JsonSerializer.Deserialize<TicketCommentAddedIntegrationEvent>(message);
                if (commentEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TicketCommentAddedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(commentEvent, ct);
                }
                break;

            case nameof(EmailReceivedIntegrationEvent):
                var emailEvent = JsonSerializer.Deserialize<EmailReceivedIntegrationEvent>(message);
                if (emailEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<EmailReceivedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(emailEvent, ct);
                }
                break;

            case nameof(SlaBreachedIntegrationEvent):
                var slaEvent = JsonSerializer.Deserialize<SlaBreachedIntegrationEvent>(message);
                if (slaEvent != null)
                {
                    var handlers = serviceProvider.GetServices<IIntegrationEventHandler<SlaBreachedIntegrationEvent>>();
                    foreach (var handler in handlers)
                        await handler.HandleAsync(slaEvent, ct);
                }
                break;

            default:
                _logger.LogWarning("Unknown integration event name received on EventBus: {EventName}", eventName);
                break;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);
        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
