using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public DeleteTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");

        ticket.Delete();
        ticket.AddActivity("Ticket Deleted", null, "Soft Deleted", request.PerformedBy);
        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}
