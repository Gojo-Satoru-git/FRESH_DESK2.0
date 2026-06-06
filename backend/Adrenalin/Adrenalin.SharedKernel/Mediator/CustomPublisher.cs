using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Default <see cref="IPublisher"/> implementation.
/// <para>
/// Uses a <see cref="ConcurrentDictionary{TKey,TValue}"/> to cache a strongly-typed
/// <see cref="NotificationHandlerWrapper"/> per concrete notification type, constructed
/// once via reflection and reused for every subsequent publish — matching the same
/// zero-reflection-at-runtime strategy as <see cref="CustomDispatcher"/>.
/// </para>
/// <para>
/// All handlers for a notification type are resolved from the DI scope and invoked
/// sequentially. No handler failure short-circuits the others by default.
/// </para>
/// </summary>
public sealed class CustomPublisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Shared across all <see cref="CustomPublisher"/> instances — one wrapper per
    /// notification type, created once and reused for every subsequent publish.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _cache = new();

    public CustomPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => PublishCore(notification, cancellationToken);

    /// <inheritdoc/>
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        => PublishCore(notification, cancellationToken);

    private Task PublishCore(INotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var wrapper = _cache.GetOrAdd(notification.GetType(), type =>
        {
            var wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(type);
            return (NotificationHandlerWrapper)(Activator.CreateInstance(wrapperType)
                   ?? throw new InvalidOperationException($"Could not create notification wrapper for {type.Name}"));
        });

        return wrapper.Publish(notification, _serviceProvider, cancellationToken);
    }
}

/// <summary>
/// Non-generic base allowing <see cref="CustomPublisher"/> to store wrappers of different
/// closed notification types in a single dictionary.
/// </summary>
internal abstract class NotificationHandlerWrapper
{
    public abstract Task Publish(INotification notification, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

/// <summary>
/// Closed-generic wrapper created once per concrete notification type.
/// Resolves all <see cref="INotificationHandler{TNotification}"/> registrations from DI
/// and invokes them sequentially.
/// </summary>
internal sealed class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override async Task Publish(INotification notification, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var handler in handlers)
            await handler.Handle((TNotification)notification, cancellationToken);
    }
}
