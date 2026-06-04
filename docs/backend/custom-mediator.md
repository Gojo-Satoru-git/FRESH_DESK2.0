# Custom Mediator Pattern (MediatR Replacement)

This documentation provides an architectural overview and integration instructions for the custom, lightweight Mediator pattern implementation built to replace `MediatR` in Adrenalin.

For the C# implementation code, see [Adrenalin.SharedKernel.Mediator](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/).

---

## Why Custom Mediator?
We wanted to replace the third-party `MediatR` dependency to avoid licensing constraints and to optimize request dispatch execution path. The custom dispatcher provides:
1. **No License Warnings**: Complete, 100% open-source custom codebase.
2. **Superior Performance**: Cached generic wrappers ensure zero runtime reflection during dispatch.
3. **Low-Allocation**: O(1) Concurrent Dictionary lookups match requests directly to their wrappers.
4. **Russian-Doll Decorators**: Composes validation, auditing, logging, and transactions cleanly using standard C# delegates.

---

## Architectural Component Breakdown

### 1. Request Types
Any request, query, or command must implement the [IRequest\<TResponse\>](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/IRequest.cs) interface:
```csharp
namespace Adrenalin.SharedKernel.Mediator;

public interface IRequest<out TResponse> { }
```

### 2. Request Handlers
Each request is handled by a class implementing [IRequestHandler\<TRequest, TResponse\>](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/IRequestHandler.cs):
```csharp
namespace Adrenalin.SharedKernel.Mediator;

public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

### 3. Pipeline Behaviors (Middleware)
Cross-cutting concerns are mapped using [IPipelineBehavior\<TRequest, TResponse\>](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/IPipelineBehavior.cs):
```csharp
namespace Adrenalin.SharedKernel.Mediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken);
}
```

### 4. Dispatcher Core
The [IDispatcher](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/IDispatcher.cs) represents the entry point. Its implementation [CustomDispatcher](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/CustomDispatcher.cs) manages caching of strongly-typed wrapper classes to route the requests.

---

## How to Register Handlers and Behaviors

### Assembly Scanning
To register all handlers and behaviors within an assembly, invoke the extension method [AddCustomDispatcher](../../backend/Adrenalin/Adrenalin.SharedKernel/Mediator/DispatcherServiceCollectionExtensions.cs):
```csharp
using Adrenalin.SharedKernel.Mediator;
using System.Reflection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomDispatcher(Assembly.GetExecutingAssembly());
        return services;
    }
}
```

### Manual Generic Registrations
If registering open-generic pipeline behaviors manually:
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

---

## Execution Pipeline Mechanics (Russian-Doll)

When `_dispatcher.Send(command)` is invoked:
1. `CustomDispatcher` queries its cache for the `RequestHandlerWrapper` corresponding to the concrete command type.
2. If not cached, it constructs `RequestHandlerWrapperImpl<TRequest, TResponse>` via reflection once and stores it.
3. The wrapper resolves `IRequestHandler<TRequest, TResponse>` and all registered `IPipelineBehavior<TRequest, TResponse>` instances.
4. The execution chain is composed in reverse order:
   ```csharp
   RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle(request, cancellationToken);

   foreach (var behavior in behaviors.Reverse())
   {
       var next = handlerDelegate;
       handlerDelegate = () => behavior.Handle(request, next, cancellationToken);
   }
   ```
5. Composed delegate starts executing. The first registered behavior executes first, wraps the next behavior, which wraps the request handler.
