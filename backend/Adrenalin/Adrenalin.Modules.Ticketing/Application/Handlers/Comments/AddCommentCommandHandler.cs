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

        var comment = TicketComment.Create(
            request.TicketId,
            request.AuthorId,
            request.ContactId,
            request.Body,
            request.Visibility
        );

        var modifiedBy = request.AuthorId ?? request.ContactId!.Value;

        ticket.AddComment(comment, modifiedBy);

        return comment.Id;
    }
}