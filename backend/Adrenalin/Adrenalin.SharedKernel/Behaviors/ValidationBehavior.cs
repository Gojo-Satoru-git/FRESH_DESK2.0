using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.SharedKernel.Behaviors
{
    public sealed class ValidationBehavior<TRequest,TResponse>:IPipelineBehavior<TRequest,TResponse> where TRequest:IRequest<TResponse>
    {
        private readonly IEnumerable< IValidator<TRequest>> _validator;
        public ValidationBehavior(IEnumerable< IValidator<TRequest>> validators)
        {
            _validator=validators;
        }
        public async Task<TResponse> Handle(TRequest request,RequestHandlerDelegate<TResponse> next,CancellationToken cancellationToken)
        {
            if (_validator.Any())
            {
                var context=new ValidationContext<TRequest>(request);
                var failure=_validator.Select(v=>v.Validate(context))
                                      .SelectMany(r=>r.Errors)
                                      .Where(f=>f!=null)
                                      .ToList();
                if (failure.Count != 0)
                {
                    var errorsDictionary = failure
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    throw new Adrenalin.SharedKernel.Exceptions.ValidationException(errorsDictionary);
                }
            }
            return await next();
        }
    }
}