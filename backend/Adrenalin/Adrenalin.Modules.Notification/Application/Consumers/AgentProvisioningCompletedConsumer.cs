using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.Consumers;

public class AgentProvisioningCompletedConsumer : IIntegrationEventHandler<AgentProvisioningCompletedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepository;

    public AgentProvisioningCompletedConsumer(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task HandleAsync(AgentProvisioningCompletedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        // 1. Create a log entity for the operation status
        var log = new NotificationLog
        {
            RecipientEmail = @event.AdminEmail,
            IsFailedDelivery = false,
            ErrorMessage = $"Agent {@event.DisplayName} ({@event.AgentEmail}) has been successfully created and configured.",
            SentAt = DateTime.UtcNow,
            TemplateId = Guid.Parse("8c2b5bc1-4da2-4b2e-9cf4-fa5f3366afa6") // ◄ Real seeded database key
        };

        // 2. Persist to database via your repository
        await _notificationRepository.AddLogAsync(log, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}