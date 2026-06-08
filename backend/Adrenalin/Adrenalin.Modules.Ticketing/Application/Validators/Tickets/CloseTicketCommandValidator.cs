using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class CloseTicketCommandValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.ClosedBy).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Closing notes cannot exceed 1000 characters.");
    }
}
