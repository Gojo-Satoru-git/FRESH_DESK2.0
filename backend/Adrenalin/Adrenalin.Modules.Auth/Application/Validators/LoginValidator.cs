using FluentValidation;
using Adrenalin.Modules.Auth.Application.Commands;

namespace Adrenalin.Modules.Auth.Application.Validators;

public sealed class LoginValidator
    : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}