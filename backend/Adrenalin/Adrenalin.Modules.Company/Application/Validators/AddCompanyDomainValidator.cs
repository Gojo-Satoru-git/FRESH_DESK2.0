using Adrenalin.Modules.Company.Application.Commands;
using FluentValidation;

public sealed class AddCompanyDomainValidator : AbstractValidator<AddCompanyDomainCommand>
{
    public AddCompanyDomainValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Domain).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}
