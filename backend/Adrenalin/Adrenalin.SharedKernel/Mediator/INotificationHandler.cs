namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Handles a notification (domain event) of type <typeparamref name="TNotification"/>.
/// <para>
/// Multiple handlers may be registered for the same notification type.
/// <see cref="IPublisher"/> resolves and invokes all of them sequentially.
/// </para>
/// </summary>
/// <typeparam name="TNotification">The notification type to handle.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
