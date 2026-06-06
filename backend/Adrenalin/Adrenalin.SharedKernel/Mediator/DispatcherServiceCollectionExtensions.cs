using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.SharedKernel.Mediator;

public static class DispatcherServiceCollectionExtensions
{
    public static IServiceCollection AddCustomDispatcher(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("Assemblies to scan must be provided.", nameof(assemblies));
        }

        services.AddScoped<IDispatcher, CustomDispatcher>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var handlerInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.AddTransient(handlerInterface, type);
                }

                var behaviorInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

                if (behaviorInterfaces.Any())
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        services.AddTransient(typeof(IPipelineBehavior<,>), type);
                    }
                    else
                    {
                        foreach (var behaviorInterface in behaviorInterfaces)
                        {
                            services.AddTransient(behaviorInterface, type);
                        }
                    }
                }
            }
        }

        return services;
    }
}
