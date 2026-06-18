using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;
using FluentValidation.Validators;
using Adrenalin.SharedKernel.Mediator;
namespace Adrenalin.Modules.Auth.Application.Validators
{
    public class RegisterUserValidator:AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x=>x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

            RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d")
            .WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

            RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

            RuleFor(x => x.Username)
            .MaximumLength(50);

            RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^[0-9]{10}$")
            .WithMessage("Phone number must contain exactly 10 digits.");
        }
    }
}