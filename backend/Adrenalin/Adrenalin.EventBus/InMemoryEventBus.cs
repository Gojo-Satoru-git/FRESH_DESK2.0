using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adrenalin.EventBus;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("InMemoryEventBus: Publishing event {EventName}", typeof(T).Name);

        // Run integration handlers asynchronously in a background task to mimic out-of-process behavior
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<T>>().ToList();
                 _logger.LogInformation(
                "Found {Count} handlers for {EventName}",
                handlers.Count,
                typeof(T).Name);
                
                foreach (var handler in handlers)
                {
                    _logger.LogInformation("InMemoryEventBus: Routing {EventName} to handler {HandlerName}", typeof(T).Name, handler.GetType().Name);
                    await handler.HandleAsync(integrationEvent, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InMemoryEventBus: Error executing handler for event {EventName}", typeof(T).Name);
            }
        });

        return Task.CompletedTask;
    }
}
