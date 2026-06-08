using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators.Tickets;

public sealed class DeleteTicketCommandValidator : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketCommandValidator()
    {
        RuleFor(v => v.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(v => v.PerformedBy)
            .NotEmpty().WithMessage("PerformedBy is required.");
    }
}
