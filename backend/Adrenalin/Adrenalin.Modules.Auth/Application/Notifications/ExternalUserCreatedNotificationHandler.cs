using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Notifications
{
    public class ExternalUserCreatedNotificationHandler: INotificationHandler<ExternalUserCreatedNotification>
    {
         private readonly IEventBus _eventBus;

    public ExternalUserCreatedNotificationHandler(
        IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(
        ExternalUserCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("NOTIFICATION HANDLER HIT");
        await _eventBus.PublishAsync(
            new ExternalUserCreatedEvent(
                notification.UserId,
                notification.CompanyId,
                notification.Email,
                notification.FullName,
                notification.CreatedBy),
            cancellationToken);
    }
    }
}