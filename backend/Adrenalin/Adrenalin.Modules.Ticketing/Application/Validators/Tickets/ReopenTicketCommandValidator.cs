using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class ReopenTicketCommandValidator : AbstractValidator<ReopenTicketCommand>
{
    public ReopenTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.ReopenedBy).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reason for reopening is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
