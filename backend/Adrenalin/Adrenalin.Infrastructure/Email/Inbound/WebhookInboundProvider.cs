using System.Collections.Generic;
using Adrenalin.EventBus.Events;

namespace Adrenalin.Infrastructure.Email.Inbound;

public sealed class WebhookInboundProvider : IInboundEmailProvider
{
    public string ProviderName => "Webhook";

    public Task<IReadOnlyList<EmailReceivedIntegrationEvent>> ReceiveEmailsAsync(CancellationToken cancellationToken = default)
    {
        // Webhooks are pushed via API controllers, so polling is not required here.
        return Task.FromResult<IReadOnlyList<EmailReceivedIntegrationEvent>>(Array.Empty<EmailReceivedIntegrationEvent>());
    }
}
