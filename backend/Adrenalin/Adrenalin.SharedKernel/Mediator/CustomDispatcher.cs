using System.Collections.Concurrent;

namespace Adrenalin.SharedKernel.Mediator;

/// <summary>
/// Default <see cref="IDispatcher"/> implementation.
/// <para>
/// Uses a <see cref="ConcurrentDictionary{TKey,TValue}"/> to cache a strongly-typed
/// <see cref="RequestHandlerWrapper{TResponse}"/> per concrete request type, so reflection
/// is only paid once per type — subsequent dispatches are O(1) dictionary lookups with
/// no runtime allocation overhead.
/// </para>
/// <para>
/// Pipeline execution (Russian-doll): behaviors are composed in reverse-registration order so
/// that the first registered behavior is the outermost decorator and executes first.
/// </para>
/// </summary>
public sealed class CustomDispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Shared across all <see cref="CustomDispatcher"/> instances — one wrapper per request type,
    /// constructed once via reflection and reused for every subsequent dispatch.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> _wrapperCache = new();

    public CustomDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var wrapper = _wrapperCache.GetOrAdd(requestType, type =>
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(type, typeof(TResponse));
            return Activator.CreateInstance(wrapperType)
                   ?? throw new InvalidOperationException($"Could not create handler wrapper for {type.Name}");
        });

        return ((RequestHandlerWrapper<TResponse>)wrapper).Handle(request, _serviceProvider, cancellationToken);
    }
}
