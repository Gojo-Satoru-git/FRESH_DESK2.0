using Microsoft.Extensions.DependencyInjection;

namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Non-generic base that lets <see cref="CustomDispatcher"/> store wrappers of different
/// closed generic types in a single <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
/// Closed-generic wrapper created once per concrete request type.
/// Resolves the handler and all pipeline behaviors from DI, then composes them
/// into a Russian-doll delegate chain before invoking.
/// </summary>
public sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public override async Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        // Innermost delegate: the actual handler.
        RequestHandlerDelegate<TResponse> handlerDelegate =
            () => handler.Handle((TRequest)request, cancellationToken);

        // Wrap each behavior around the previous delegate (reverse order = outermost first).
        foreach (var behavior in behaviors.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((TRequest)request, next, cancellationToken);
        }

        return await handlerDelegate();
    }
}
