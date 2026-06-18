using Adrenalin.EventBus.Events;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public interface IEmailThreadDetectionService
{
    Task<Guid?> DetectThreadAsync(EmailReceivedIntegrationEvent integrationEvent, Guid companyId, Guid? senderUserId, CancellationToken cancellationToken = default);
}
