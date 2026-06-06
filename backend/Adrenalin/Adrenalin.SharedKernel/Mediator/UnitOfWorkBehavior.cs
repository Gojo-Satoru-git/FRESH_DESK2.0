using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.SharedKernel.Mediator;

public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (typeof(TRequest).Name.EndsWith("Command"))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
