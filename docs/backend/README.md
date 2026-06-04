# Custom Mediator Pattern (MediatR Replacement)

This is a lightweight, highly optimized custom implementation of the Mediator pattern built for .NET 8+ / .NET 10. It serves as a 100% free, open-source replacement for `MediatR`, designed to enforce clean architecture and high-performance dispatching without license key warnings or per-request reflection overhead.

---

## Architecture & Performance Features

1. **Zero Per-Request Reflection**:
   Instead of using `MakeGenericType` or dynamic invocation during request execution, the dispatcher uses a strongly-typed abstract wrapper model ([RequestHandlerWrapper.cs](RequestHandlerWrapper.cs)). Reflection is executed only once per request type.

2. **O(1) Concurrent Caching**:
   Created wrappers are cached in a thread-safe `ConcurrentDictionary<Type, object>`. Subsequent dispatches resolve the wrapper in $O(1)$ time and execute it directly.

3. **Russian-Doll Pipeline Model**:
   Supports [IPipelineBehavior.cs](IPipelineBehavior.cs) cross-cutting concern decorators (middleware). The behaviors are resolved and composed sequentially at runtime, wrapping around the inner request handler just like MediatR's pipelines work.

4. **Single-Scan DI Registration**:
   Scans provided assemblies once during startup using reflection to register all implementations of `IRequestHandler<,>` and `IPipelineBehavior<,>` in the Microsoft dependency injection container. See [DispatcherServiceCollectionExtensions.cs](DispatcherServiceCollectionExtensions.cs).

---

## Core Interfaces

### `IRequest<TResponse>`
Marker interface representing a request/command/query that returns a response of type `TResponse`. See [IRequest.cs](IRequest.cs).

### `IRequestHandler<TRequest, TResponse>`
Defines the handler logic for a specific request. See [IRequestHandler.cs](IRequestHandler.cs).

### `IPipelineBehavior<TRequest, TResponse>`
Allows hooking custom logic (logging, validation, transactions, etc.) around the request execution. See [IPipelineBehavior.cs](IPipelineBehavior.cs).

### `IDispatcher`
The central dispatching hub. See [IDispatcher.cs](IDispatcher.cs).

---

## Usage Guide

### 1. Registering the Dispatcher
In your assembly configuration/startup (e.g., `Program.cs` or module `DependencyInjection.cs`), register the dispatcher using assembly scanning:
```csharp
using Adrenalin.SharedKernel.Mediator;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registers IDispatcher and scans current assembly for Handlers & Behaviors
        services.AddCustomDispatcher(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
```

### 2. Defining a Command or Query
Create a message record or class implementing `IRequest<TResponse>`:
```csharp
using Adrenalin.SharedKernel.Mediator;

public sealed record CreateTicketCommand(
    Guid CompanyId, 
    Guid ModuleId, 
    string Subject, 
    string Description) : IRequest<Guid>;
```

### 3. Writing a Handler
Create a class implementing `IRequestHandler<TRequest, TResponse>`:
```csharp
using Adrenalin.SharedKernel.Mediator;

public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        // Business logic here...
        return Guid.NewGuid();
    }
}
```

### 4. Implementing Pipeline Behaviors (Middleware)
Create pipeline behavior classes to decorate request execution. For example, a validation behavior:
```csharp
using Adrenalin.SharedKernel.Mediator;
using FluentValidation;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(); // Proceed to next behavior or request handler
    }
}
```

> [!NOTE]
> Pipeline behaviors are executed in the order they are registered in the DI container. The first registered behavior runs first (starts the outer shell of the Russian-doll), and then delegates to the next behavior until the actual request handler is executed.

### 5. Dispatching Requests
Inject `IDispatcher` into your controller, middleware, or service:
```csharp
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TicketsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketCommand command)
    {
        var id = await _dispatcher.Send(command);
        return Ok(id);
    }
}
```
