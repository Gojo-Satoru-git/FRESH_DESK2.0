using FluentValidation;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Adrenalin.Modules.Ticketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketingApplication(
        this IServiceCollection services)
    {
         services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
         services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // Scans assembly → auto-registers ALL IRequestHandler<,>
        // and ALL IPipelineBehavior<,> — no manual handler registration needed
        services.AddCustomDispatcher(Assembly.GetExecutingAssembly());

        // Validators — once only
        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        // Pipeline behaviour — UnitOfWork wraps every command
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        // Assignment helper — not a handler so needs manual registration
        services.AddScoped<AutomationConditionEvaluator>();

        return services;
    }
}