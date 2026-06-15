using Adrenalin.Modules.Company.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Company.Application.Validators;

public sealed class UpdateContactValidator : AbstractValidator<UpdateContactCommand>
{
    public UpdateContactValidator()
    {
        RuleFor(x => x.ContactId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ModifiedBy).NotEmpty();
    }
}

public sealed class UpdateHealthScoreValidator : AbstractValidator<UpdateHealthScoreCommand>
{
    public UpdateHealthScoreValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 100);
        RuleFor(x => x.ModifiedBy).NotEmpty();
    }
}

public sealed class UpdateTierValidator : AbstractValidator<UpdateTierCommand>
{
    public UpdateTierValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupportTier).NotEmpty();
        RuleFor(x => x.ModifiedBy).NotEmpty();
    }
}

public sealed class UpdateContactLimitValidator : AbstractValidator<UpdateContactLimitCommand>
{
    public UpdateContactLimitValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MaxContacts).InclusiveBetween(1, 100);
        RuleFor(x => x.ModifiedBy).NotEmpty();
    }
}
