using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.Modules.Ticketing.Application.Commands;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public class EmailWatcherExtractionService : IEmailWatcherExtractionService
{
    private readonly IDispatcher _dispatcher;
    private readonly Microsoft.Extensions.Logging.ILogger<EmailWatcherExtractionService> _logger;
    private const int MaxWatchers = 10;
    
    // Basic email regex for format validation
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public EmailWatcherExtractionService(IDispatcher dispatcher, Microsoft.Extensions.Logging.ILogger<EmailWatcherExtractionService> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<List<Guid>> ExtractWatchersAsync(string? toEmails, IEnumerable<string>? ccEmails, string senderEmail, string systemEmail, Guid companyId, CancellationToken cancellationToken = default)
    {
        var rawEmails = new List<string>();

        if (!string.IsNullOrWhiteSpace(toEmails))
        {
            rawEmails.AddRange(toEmails.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        if (ccEmails != null)
        {
            foreach (var cc in ccEmails)
            {
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    rawEmails.AddRange(cc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        var candidateEmails = rawEmails
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => EmailRegex.IsMatch(e))
            .Distinct()
            .Where(e => !string.Equals(e, senderEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(e => !string.Equals(e, systemEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(MaxWatchers)
            .ToList();

        var watcherIds = new List<Guid>();

        foreach (var email in candidateEmails)
        {
            var resolveCommand = new ResolveEmailContactContractCommand(email, "", null);
            var resolveResult = await _dispatcher.Send(resolveCommand, cancellationToken);

            if (resolveResult.IsSuccess && resolveResult.Value != null)
            {
                if (resolveResult.Value.UserId.HasValue && resolveResult.Value.UserId.Value != Guid.Empty)
                {
                    // Watchers should only be internal agents. External contacts should not be watchers.
                    if (resolveResult.Value.CompanyId == companyId)
                    {
                        _logger.LogInformation("Contact resolved for CC email {Email} belongs to the customer company. Skipping watcher creation.", email);
                    }
                    else
                    {
                        watcherIds.Add(resolveResult.Value.UserId.Value);
                    }
                }
                else
                {
                    _logger.LogInformation("Contact resolved for CC email {Email} does not have a linked UserId. Skipping watcher creation.", email);
                }
            }
        }

        return watcherIds;
    }
}
