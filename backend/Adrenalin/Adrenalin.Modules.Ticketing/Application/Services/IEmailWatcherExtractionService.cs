using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public interface IEmailWatcherExtractionService
{
    Task<List<Guid>> ExtractWatchersAsync(string? toEmails, IEnumerable<string>? ccEmails, string senderEmail, string systemEmail, Guid companyId, CancellationToken cancellationToken = default);
}
