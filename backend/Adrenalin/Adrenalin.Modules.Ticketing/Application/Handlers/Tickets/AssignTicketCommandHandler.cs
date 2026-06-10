using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using System.Linq;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public AssignTicketCommandHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
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

        var roles = _currentUserService.Roles.ToList();
        if (roles.Contains("junior_agent", StringComparer.OrdinalIgnoreCase) || 
            roles.Contains("senior_agent", StringComparer.OrdinalIgnoreCase))
        {
            throw new TicketDomainException("Agents are not authorized to assign tickets.");
        }

        var isAssignerInternal = roles.Any(r => new[] { "admin", "team_lead", "manager", "pmo" }.Contains(r, StringComparer.OrdinalIgnoreCase));
        if (!isAssignerInternal)
        {
            var assignerCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.AssignedBy, cancellationToken);
            if (assignerCompanyId != ticket.CompanyId)
            {
                throw new TicketDomainException("Assigner does not belong to the ticket's company.");
            }

            // External assigners (like customer_admin) can only assign users within their own company
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