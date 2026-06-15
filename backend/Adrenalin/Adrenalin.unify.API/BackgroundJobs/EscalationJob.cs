using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.SLA.Application.Commands;

namespace Adrenalin.unify.API.BackgroundJobs;

public sealed class EscalationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory; // ⚡ Inject the factory rather than direct dependencies
    private readonly ILogger<EscalationJob> _logger;

    public EscalationJob(IServiceScopeFactory scopeFactory, ILogger<EscalationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Escalation Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking SLA escalation rules...");

                // ⚡ CREATE A CLEAN ISOLATED DI SCOPE FOR THIS TICK:
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Resolve dependencies cleanly out of the isolated scope container
                    var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

                    // Dispatches the rule engine cleanly
                    await dispatcher.Send(new CheckEscalationsCommand(), stoppingToken);
                } // 🎯 The using block ends here: The DbContext connection flushes and drops right back into the Npgsql pool immediately!

                // Sleep interval window tracking delay
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Escalation worker error encountered.");

                // Backoff cool-down delay to avoid slamming the database during a brief network drop
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}