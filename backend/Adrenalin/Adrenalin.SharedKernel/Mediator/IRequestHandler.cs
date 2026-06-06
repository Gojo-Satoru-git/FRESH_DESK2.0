namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Defines a handler for a <typeparamref name="TRequest"/>.
/// Each request must have exactly one handler registered in the DI container.
/// </summary>
/// <typeparam name="TRequest">The request type to handle.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
