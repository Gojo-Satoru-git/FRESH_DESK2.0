using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();

        RuleFor(x => x.ModuleId).NotEmpty();

        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(10000);
    }
}