using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public ChangeTicketStatusCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if(ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' not found.");
        }

        switch (request.NewStatus)
        {
            case TicketStatus.Resolved:
                ticket.Resolve(request.ChangedBy, request.Reason ?? string.Empty);
                break;
            case TicketStatus.Closed:
                ticket.Close(request.ChangedBy, request.Reason ?? string.Empty);
                break;
            case TicketStatus.Reopened:
                ticket.Reopen(request.ChangedBy, request.Reason ?? string.Empty);
                break;
            default:
                ticket.ChangeStatus(request.NewStatus, request.ChangedBy, request.Reason);
                break;
        }

        return ticket.Id;
    }
}