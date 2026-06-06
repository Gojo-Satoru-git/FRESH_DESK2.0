using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class ReopenTicketCommandHandler : IRequestHandler<ReopenTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public ReopenTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(ReopenTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var userCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.ReopenedBy, cancellationToken);
        if (userCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("User does not belong to the ticket's company.");
        }

        ticket.Reopen(request.ReopenedBy, request.Reason);

        return ticket.Id;
    }
}
