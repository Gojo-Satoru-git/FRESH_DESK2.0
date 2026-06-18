using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus.Events;

namespace Adrenalin.Infrastructure.Email.Inbound;

public interface IInboundEmailProvider
{
    string ProviderName { get; }
    Task<IReadOnlyList<EmailReceivedIntegrationEvent>> ReceiveEmailsAsync(CancellationToken cancellationToken = default);
}
