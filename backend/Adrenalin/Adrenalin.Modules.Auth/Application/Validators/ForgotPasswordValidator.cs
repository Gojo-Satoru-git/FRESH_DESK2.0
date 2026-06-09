using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
          public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
    }
}