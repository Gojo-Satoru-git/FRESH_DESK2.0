using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        
        RuleFor(x => x.AgentId).NotEmpty();
        
        RuleFor(x => x.AssignedBy).NotEmpty();

        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}