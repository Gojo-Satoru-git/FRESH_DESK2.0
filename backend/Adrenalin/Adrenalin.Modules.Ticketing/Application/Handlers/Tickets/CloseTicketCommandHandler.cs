using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using System.Linq;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public CloseTicketCommandHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var roles = _currentUserService.Roles.ToList();
        var isCustomer = roles.Contains("customer", StringComparer.OrdinalIgnoreCase);

        if (isCustomer && ticket.CreatedByUserId != request.ClosedBy)
        {
            throw new TicketDomainException("Customers can only close their own tickets.");
        }

        var isInternal = roles.Any(r => new[] { "admin", "team_lead", "manager", "pmo", "senior_agent", "junior_agent" }
            .Contains(r, StringComparer.OrdinalIgnoreCase));

        if (!isInternal)
        {
            var userCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.ClosedBy, cancellationToken);
            if (userCompanyId != ticket.CompanyId)
            {
                throw new TicketDomainException("User does not belong to the ticket's company.");
            }
        }

        ticket.Close(request.ClosedBy, request.Notes);

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}
