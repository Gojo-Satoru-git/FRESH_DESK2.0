using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class ChangeTicketStatusCommandValidator : AbstractValidator<ChangeTicketStatusCommand>
{
    public ChangeTicketStatusCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.ChangedBy).NotEmpty();

        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}