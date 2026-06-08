using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators.Tickets;

public sealed class ResolveTicketCommandValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketCommandValidator()
    {
        RuleFor(v => v.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(v => v.ResolvedBy)
            .NotEmpty().WithMessage("ResolvedBy is required.");

        RuleFor(v => v.ResolutionSummary)
            .MaximumLength(1000).WithMessage("Resolution summary must not exceed 1000 characters.");
    }
}
