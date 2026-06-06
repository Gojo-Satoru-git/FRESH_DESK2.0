using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class ResolveTicketCommandHandler : IRequestHandler<ResolveTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public ResolveTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(ResolveTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var userCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.ResolvedBy, cancellationToken);
        if (userCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("User does not belong to the ticket's company.");
        }

        ticket.Resolve(request.ResolvedBy, request.ResolutionSummary);

        return ticket.Id;
    }
}
