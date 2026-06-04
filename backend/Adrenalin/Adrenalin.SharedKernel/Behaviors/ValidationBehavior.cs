using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Adrenalin.SharedKernel.Behaviors
{
    public sealed class ValidationBehavior<TRequest,TResponse>:IPipelineBehavior<TRequest,TResponse> where TRequest:notnull
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
                    throw new ValidationException(failure);
                }
            }
            return await next();
        }
    }
}