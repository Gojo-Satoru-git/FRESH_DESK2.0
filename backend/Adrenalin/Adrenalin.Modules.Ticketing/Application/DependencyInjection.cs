using FluentValidation;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Adrenalin.Modules.Ticketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketingApplication(this IServiceCollection services)
    {
        services.AddCustomDispatcher(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}