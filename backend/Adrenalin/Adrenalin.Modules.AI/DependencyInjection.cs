using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Infrastructure;

namespace Adrenalin.Modules.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAiModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddScoped<IAiProvider, MockAiProvider>();

        return services;
    }
}
