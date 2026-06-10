using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public class ResolveTicketCommandValidator
    : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("TicketId is required");

        RuleFor(x => x.AgentId)
            .NotEmpty()
            .WithMessage("AgentId is required");

        RuleFor(x => x.ResolutionNote)
            .NotEmpty()
            .WithMessage("Resolution note is required")
            .MinimumLength(10)
            .WithMessage("Resolution note must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Resolution note cannot exceed 1000 characters");
    }
}