using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

public sealed class AddWatcherCommandHandler : IRequestHandler<AddWatcherCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public AddWatcherCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(AddWatcherCommand request, CancellationToken cancellationToken)
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

        var addedByCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.AddedBy, cancellationToken);
        if (addedByCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("The user adding the watcher must belong to the same company as the ticket.");
        }

        ticket.AddWatcher(request.UserId, request.AddedBy);

        return ticket.Id;
    }
}