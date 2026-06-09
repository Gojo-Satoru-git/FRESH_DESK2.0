using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class RemoveWatcherCommandHandler : IRequestHandler<RemoveWatcherCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    public RemoveWatcherCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(RemoveWatcherCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var watcherCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.UserId, cancellationToken);
        
        if (watcherCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("Watcher user must belong to the same company as the ticket.");
        }

        ticket.RemoveWatcher(request.UserId);

        return ticket.Id;
    }
}