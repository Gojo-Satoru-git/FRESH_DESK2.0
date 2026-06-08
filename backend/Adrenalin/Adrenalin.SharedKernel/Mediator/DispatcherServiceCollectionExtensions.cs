using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Extension methods for registering the custom mediator with the DI container.
/// </summary>
public static class DispatcherServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IDispatcher"/> (<see cref="CustomDispatcher"/>) and
    /// <see cref="IPublisher"/> (<see cref="CustomPublisher"/>) as scoped services,
    /// then scans <paramref name="assemblies"/> to auto-register:
    /// <list type="bullet">
    ///   <item><see cref="IRequestHandler{TRequest,TResponse}"/> implementations (transient)</item>
    ///   <item><see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations (transient)</item>
    ///   <item><see cref="INotificationHandler{TNotification}"/> implementations (transient)</item>
    /// </list>
    /// <para>
    /// Open-generic pipeline behaviors (e.g. <c>LoggingBehavior&lt;,&gt;</c>) are registered
    /// against the open-generic service type so the DI container closes them for every request.
    /// If you prefer to register them manually, use:
    /// <code>services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;));</code>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">One or more assemblies to scan for handler and behavior implementations.</param>
    /// <example>
    /// Typical usage in a module's DI wiring:
    /// <code>
    /// public static IServiceCollection AddApplication(this IServiceCollection services)
    /// {
    ///     services.AddCustomDispatcher(Assembly.GetExecutingAssembly());
    ///     return services;
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddCustomDispatcher(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            throw new ArgumentException("Assemblies to scan must be provided.", nameof(assemblies));

        services.AddScoped<IDispatcher, CustomDispatcher>();
        services.AddScoped<IPublisher, CustomPublisher>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                // ── IRequestHandler<TRequest, TResponse> ──────────────────────
                var handlerInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                foreach (var handlerInterface in handlerInterfaces)
                    services.AddTransient(handlerInterface, type);

                // ── IPipelineBehavior<TRequest, TResponse> ────────────────────
                var behaviorInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

                if (behaviorInterfaces.Any())
                {
                    // Open-generic behaviors (e.g. ValidationBehavior<,>) must be registered
                    // against the open-generic service type so the container can close them.
                    if (type.IsGenericTypeDefinition)
                        services.AddTransient(typeof(IPipelineBehavior<,>), type);
                    else
                        foreach (var behaviorInterface in behaviorInterfaces)
                            services.AddTransient(behaviorInterface, type);
                }

                // ── INotificationHandler<TNotification> ──────────────────────
                var notificationInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

                foreach (var notificationInterface in notificationInterfaces)
                    services.AddTransient(notificationInterface, type);
            }
        }

        return services;
    }
}
