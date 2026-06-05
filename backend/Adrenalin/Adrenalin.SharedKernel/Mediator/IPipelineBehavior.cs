namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Represents the next step in the pipeline. Invoking this delegate advances execution
/// to the next behavior, or to the request handler if no more behaviors remain.
/// </summary>
/// <typeparam name="TResponse">The response type of the current request.</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Defines a pipeline behavior (middleware) that wraps request handling.
/// Cross-cutting concerns such as validation, logging, auditing, and transactions
/// are implemented by registering one or more <see cref="IPipelineBehavior{TRequest,TResponse}"/>
/// types in the DI container. They compose in a Russian-doll (decorator) pattern:
/// the first registered behavior is the outermost layer.
/// </summary>
/// <typeparam name="TRequest">The request type flowing through the pipeline.</typeparam>
/// <typeparam name="TResponse">The response type produced by the pipeline.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
