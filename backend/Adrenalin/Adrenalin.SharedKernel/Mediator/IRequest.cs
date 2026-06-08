namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Marker interface for a request that returns <typeparamref name="TResponse"/>.
/// Any request, query, or command must implement this interface.
/// </summary>
/// <typeparam name="TResponse">The type of response produced by this request.</typeparam>
public interface IRequest<out TResponse> { }
