namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Publishes a notification to all registered <see cref="INotificationHandler{TNotification}"/>
/// instances for that notification type.
/// <para>
/// Handlers are invoked sequentially. The concrete implementation
/// <see cref="CustomPublisher"/> caches a per-type wrapper so the handler resolution
/// path uses a single O(1) dictionary lookup after the first publish of each type.
/// </para>
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes <paramref name="notification"/> to all handlers registered for
    /// <typeparamref name="TNotification"/>.
    /// </summary>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publishes from a base <see cref="INotification"/> reference.
    /// Useful when iterating a heterogeneous list of domain events (e.g. in
    /// <c>AppDbContext.DispatchDomainEventsAsync</c>).
    /// </summary>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
