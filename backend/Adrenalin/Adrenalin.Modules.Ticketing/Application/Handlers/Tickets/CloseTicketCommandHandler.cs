using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public CloseTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var userCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.ClosedBy, cancellationToken);
        if (userCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("User does not belong to the ticket's company.");
        }

        ticket.Close(request.ClosedBy, request.Notes);

        return ticket.Id;
    }
}
