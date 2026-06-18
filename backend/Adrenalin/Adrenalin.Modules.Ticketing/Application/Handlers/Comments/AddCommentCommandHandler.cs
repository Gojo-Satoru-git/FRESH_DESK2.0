using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using System.Linq;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddCommentCommandHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var roles = _currentUserService.Roles.ToList();
        var isAgent = roles.Contains("junior_agent", StringComparer.OrdinalIgnoreCase) || 
                      roles.Contains("senior_agent", StringComparer.OrdinalIgnoreCase);

        var modifiedBy = request.AuthorId ?? request.ContactId
            ?? throw new TicketDomainException("Either AuthorId or ContactId must be provided.");

        if (isAgent && ticket.AssignedAgentId != modifiedBy)
        {
            throw new TicketDomainException("Agents can only add comments to tickets assigned to them.");
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
            null,
            request.IsPrivate
        );

        ticket.AddComment(comment, modifiedBy);

        return comment.Id;
    }
}
