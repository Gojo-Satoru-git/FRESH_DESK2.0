using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators
{
    public class ResetPasswordValidator: AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .MinimumLength(12);
    }
    }
}