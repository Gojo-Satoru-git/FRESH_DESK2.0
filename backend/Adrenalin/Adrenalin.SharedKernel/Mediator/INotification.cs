namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Marker interface for domain events and notifications.
/// <para>
/// Implement <see cref="INotificationHandler{TNotification}"/> to react to a notification.
/// Multiple handlers per notification type are supported and will all be invoked by
/// <see cref="IPublisher"/>.
/// </para>
/// </summary>
public interface INotification { }
