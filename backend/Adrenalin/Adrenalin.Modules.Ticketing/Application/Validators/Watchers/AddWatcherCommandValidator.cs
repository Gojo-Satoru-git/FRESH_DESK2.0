using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;
public sealed class AddWatcherCommandValidator : AbstractValidator<AddWatcherCommand>
{
    public AddWatcherCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.AddedBy).NotEmpty();
    }
}