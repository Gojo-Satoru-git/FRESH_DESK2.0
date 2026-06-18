using FluentValidation;
using Adrenalin.Modules.Ticketing.Application.Commands;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public class AssignTicketCommandValidator
    : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("TicketId is required");

        When(x => !x.IsAutoAssignment, () =>
        {
            RuleFor(x => x.TriggeredBy)
                .NotEmpty()
                .WithMessage("TriggeredBy is required for manual assignment");

            RuleFor(x => x)
                .Must(x => x.OverrideAgentId.HasValue
                        || x.OverrideGroupId.HasValue)
                .WithMessage(
                    "Manual assignment needs AgentId or GroupId");
        });
    }
}