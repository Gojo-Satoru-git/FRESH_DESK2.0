using Adrenalin.Modules.Company.Application.Commands;
using FluentValidation;

public sealed class CreateContactValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.CompanyId).NotEmpty();
    }
}