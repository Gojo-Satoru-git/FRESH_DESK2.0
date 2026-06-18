using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators
{
    public class ChangePasswordValidator: AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(12);

        RuleFor(x => x.NewPassword)
            .Matches("[A-Z]")
            .WithMessage("Must contain uppercase letter");

        RuleFor(x => x.NewPassword)
            .Matches("[a-z]")
            .WithMessage("Must contain lowercase letter");

        RuleFor(x => x.NewPassword)
            .Matches("[0-9]")
            .WithMessage("Must contain number");
            RuleFor(x => x.NewPassword)
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Must contain special character");
    }
    }
}