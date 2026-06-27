using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTicketCommandHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");

        var roles = _currentUserService.Roles.ToList();
        var isAgent = roles.Contains("junior_agent", StringComparer.OrdinalIgnoreCase) || 
                      roles.Contains("senior_agent", StringComparer.OrdinalIgnoreCase);

        if (isAgent && ticket.AssignedAgentId != request.ModifiedBy)
        {
            throw new TicketDomainException("Agents can only modify tickets assigned to them.");
        }

        ticket.UpdateTicket(
            request.Title,
            request.Description,
            request.Priority,
            request.Type,
            request.ModifiedBy
        );

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}
