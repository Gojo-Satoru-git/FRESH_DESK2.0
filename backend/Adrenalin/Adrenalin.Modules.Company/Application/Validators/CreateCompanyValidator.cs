using Adrenalin.Modules.Company.Application.Commands;
using FluentValidation;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.GeoRegion).NotEmpty();

        RuleFor(x => x.SupportTier).NotEmpty();

        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}