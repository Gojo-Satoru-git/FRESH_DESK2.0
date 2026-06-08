using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Repositories;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketingPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITicketRepository, TicketRepository>();

        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdrenalinDbContext>());

        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();

        return services;
    }
}