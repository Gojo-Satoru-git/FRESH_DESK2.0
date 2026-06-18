using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Adrenalin.unify.API.BackgroundJobs;

public sealed class NotificationTemplateValidatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationTemplateValidatorService> _logger;

    public NotificationTemplateValidatorService(
        IServiceProvider serviceProvider,
        ILogger<NotificationTemplateValidatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationTemplateValidatorService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ValidateTemplatesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating notification templates.");
            }

            // Run every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ValidateTemplatesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdrenalinDbContext>();

        var templates = await db.NotificationTemplates
            .Where(t => t.IsActive && !t.IsDeleted)
            .ToListAsync(ct);

        foreach (var template in templates)
        {
            bool isValid = true;
            // E.g. Check if subject or body is empty
            if (string.IsNullOrWhiteSpace(template.Subject) || string.IsNullOrWhiteSpace(template.BodyHtml))
            {
                isValid = false;
            }
            
            // Further template syntax validation can be done here.

            if (!isValid)
            {
                _logger.LogWarning("Template {TemplateCode} is invalid. Missing Subject or Body.", template.Code);
                // Optionally disable the template or send an alert to admins.
                template.Deactivate();
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
