using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class MergeTicketCommandValidator : AbstractValidator<MergeTicketCommand>
{
    public MergeTicketCommandValidator()
    {
        RuleFor(x => x.MasterTicketId).NotEmpty();

        RuleFor(x => x.DuplicateTicketId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.MasterTicketId != x.DuplicateTicketId)
            .WithMessage("A ticket cannot be merged into itself.");

        RuleFor(x => x.MergedBy).NotEmpty();
    }
}