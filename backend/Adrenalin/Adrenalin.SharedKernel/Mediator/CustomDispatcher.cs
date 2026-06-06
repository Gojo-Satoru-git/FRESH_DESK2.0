using System.Collections.Concurrent;

namespace Adrenalin.SharedKernel.Mediator;

public sealed class CustomDispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> _wrapperCache = new();

    public CustomDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
