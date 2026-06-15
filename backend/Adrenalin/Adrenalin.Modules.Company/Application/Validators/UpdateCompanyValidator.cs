using Adrenalin.Modules.Company.Application.Commands;
using FluentValidation;

public sealed class UpdateCompanyValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GeoRegion).NotEmpty();
        RuleFor(x => x.SupportTier).NotEmpty();
        RuleFor(x => x.ModifiedBy).NotEmpty();
    }
}
