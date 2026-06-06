namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// The entry point for the custom mediator. Send a request through the pipeline
/// and receive its response.
/// <para>
/// The concrete implementation <see cref="CustomDispatcher"/> routes each request to the
/// matching <see cref="IRequestHandler{TRequest,TResponse}"/>, running it through all
/// registered <see cref="IPipelineBehavior{TRequest,TResponse}"/> decorators first.
/// </para>
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches <paramref name="request"/> through the behavior pipeline and returns the result.
    /// </summary>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
