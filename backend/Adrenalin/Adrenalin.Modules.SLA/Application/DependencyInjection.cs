using Adrenalin.SharedKernel.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Adrenalin.Modules.SLA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSLAApplication(
        this IServiceCollection services)
    {
        // Auto-registers all IRequestHandler<,> in SLA assembly
        services.AddCustomDispatcher(
            Assembly.GetExecutingAssembly());

        return services;
    }
}