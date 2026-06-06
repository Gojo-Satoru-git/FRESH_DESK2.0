using Adrenalin.Modules.Ticketing.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Ticketing.Application.Validators;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.Body).NotEmpty().MaximumLength(10000);

        RuleFor(x => x)
            .Must(x => x.AuthorId.HasValue ^ x.ContactId.HasValue)
            .WithMessage("Either AuthorId or Contact must be provided.");

        RuleFor(x => x.AuthorId).NotEqual(Guid.Empty).When(x => x.AuthorId.HasValue);

        RuleFor(x => x.ContactId).NotEqual(Guid.Empty).When(x => x.ContactId.HasValue);
    }
}