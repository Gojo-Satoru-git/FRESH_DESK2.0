using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public AddCommentCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        Guid? contactId = request.ContactId;
        if (contactId.HasValue)
        {
            var contactResolved = await _ticketRepository.GetContactAndCompanyByUserIdAsync(contactId.Value, cancellationToken);
            if (contactResolved.HasValue)
            {
                contactId = contactResolved.Value.ContactId;
            }
        }

        var comment = TicketComment.Create(
            request.TicketId,
            request.AuthorId,
            contactId,
            request.Body,
            request.Visibility
        );

        var modifiedBy = request.AuthorId ?? request.ContactId
            ?? throw new TicketDomainException("Either AuthorId or ContactId must be provided.");

        ticket.AddComment(comment, modifiedBy);

        return comment.Id;
    }
}