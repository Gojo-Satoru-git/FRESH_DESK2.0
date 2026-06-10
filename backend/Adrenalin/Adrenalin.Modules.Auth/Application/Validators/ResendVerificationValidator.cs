using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators
{
    public class ResendVerificationValidator: AbstractValidator<ResendVerificationCommand>
    {
        public ResendVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
    }
}