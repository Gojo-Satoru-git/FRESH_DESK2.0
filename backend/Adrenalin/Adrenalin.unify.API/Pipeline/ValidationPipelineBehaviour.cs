using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using FluentValidation;

namespace Adrenalin.unify.API.Pipeline
{
    public sealed class ValidationPipelineBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (!_validators.Any()) return await next();

            var failures = _validators
                .Select(v => v.Validate(request))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (!failures.Any()) return await next();

            var errorMessage = string.Join(" | ", failures.Select(f => f.ErrorMessage));

            var failureMethod = typeof(TResponse)
                .GetMethod("Failure", new[] { typeof(string) })!;

            return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage })!;
        }
    }
}
