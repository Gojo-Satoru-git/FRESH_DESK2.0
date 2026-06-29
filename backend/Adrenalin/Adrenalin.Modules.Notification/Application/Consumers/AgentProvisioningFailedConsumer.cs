using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.Consumers;

public class AgentProvisioningFailedConsumer : IIntegrationEventHandler<AgentProvisioningFailedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepository;

    public AgentProvisioningFailedConsumer(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task HandleAsync(AgentProvisioningFailedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        // 1. Create a failure log entry
        var log = new NotificationLog
        {
            RecipientEmail = @event.AdminEmail,
            IsFailedDelivery = false,
            ErrorMessage = $"Agent creation pipeline crashed. Details: {@event.ErrorMessage}",
            SentAt = DateTime.UtcNow,
            TemplateId = Guid.Parse("9d3c6cd2-5eb3-4c3f-ad05-fb6f4477bfa7") // ◄ Real seeded database key
        };

        // 2. Commit error details to the logs
        await _notificationRepository.AddLogAsync(log, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}