using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Infrastructure.Email;

public interface IEmailReceive
{
    Task<IReadOnlyList<InboundEmail>> ReceiveEmailsAsync(CancellationToken cancellationToken = default);
}

public sealed record InboundEmail(
    string SenderEmail,
    string SenderName,
    string Subject,
    string Body
);
