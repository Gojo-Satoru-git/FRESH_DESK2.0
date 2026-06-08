using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public AssignTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        if (ticket.IsDeleted)
        {
            throw new TicketDomainException("Cannot assign agent to a deleted ticket.");
        }

        var isAssignerAdmin = await _ticketRepository.IsUserAdminAsync(request.AssignedBy, cancellationToken);
        if (!isAssignerAdmin)
        {
            var assignerCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.AssignedBy, cancellationToken);
            if (assignerCompanyId != ticket.CompanyId)
            {
                throw new TicketDomainException("Assigner does not belong to the ticket's company.");
            }

            var agentCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.AgentId, cancellationToken);
            if (agentCompanyId != ticket.CompanyId)
            {
                throw new TicketDomainException("Agent does not belong to the ticket's company.");
            }
        }

        ticket.AssignAgent(request.AgentId, request.AssignedBy, request.Notes);

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}