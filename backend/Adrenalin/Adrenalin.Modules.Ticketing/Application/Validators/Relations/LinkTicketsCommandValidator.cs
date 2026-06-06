using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class LinkTicketsCommandValidator : AbstractValidator<LinkTicketsCommand>
{
    public LinkTicketsCommandValidator()
    {
        RuleFor(x => x.ParentTicketId).NotEmpty();

        RuleFor(x => x.ChildTicketId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.ParentTicketId != x.ChildTicketId)
            .WithMessage("A ticket cannot be related to itself.");
    }
}