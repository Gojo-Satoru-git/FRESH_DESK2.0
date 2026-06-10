using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using System.Linq;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class ResolveTicketCommandHandler : IRequestHandler<ResolveTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public ResolveTicketCommandHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(ResolveTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var roles = _currentUserService.Roles.ToList();
        var isAgent = roles.Contains("junior_agent", StringComparer.OrdinalIgnoreCase) || 
                      roles.Contains("senior_agent", StringComparer.OrdinalIgnoreCase);

        if (isAgent && ticket.AssignedAgentId != request.ResolvedBy)
        {
            throw new TicketDomainException("Agents can only resolve tickets assigned to them.");
        }

        var isInternal = roles.Any(r => new[] { "admin", "team_lead", "manager", "pmo", "senior_agent", "junior_agent" }.Contains(r, StringComparer.OrdinalIgnoreCase));
        if (!isInternal)
        {
            var userCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.ResolvedBy, cancellationToken);
            if (userCompanyId != ticket.CompanyId)
            {
                throw new TicketDomainException("User does not belong to the ticket's company.");
            }
        }

        ticket.Resolve(request.ResolvedBy, request.ResolutionSummary);

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}
