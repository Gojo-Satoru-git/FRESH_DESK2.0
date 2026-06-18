using System.Collections.Generic;
using Adrenalin.EventBus.Events;

namespace Adrenalin.Infrastructure.Email.Inbound;

public sealed class MicrosoftGraphInboundProvider : IInboundEmailProvider
{
    public string ProviderName => "MicrosoftGraph";

    public Task<IReadOnlyList<EmailReceivedIntegrationEvent>> ReceiveEmailsAsync(CancellationToken cancellationToken = default)
    {
        // To be implemented when Graph API credentials are provided
        return Task.FromResult<IReadOnlyList<EmailReceivedIntegrationEvent>>(Array.Empty<EmailReceivedIntegrationEvent>());
    }
}
