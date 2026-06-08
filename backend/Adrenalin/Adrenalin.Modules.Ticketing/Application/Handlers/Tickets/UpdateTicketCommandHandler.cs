using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public UpdateTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");

        ticket.UpdateTicket(
            request.Title,
            request.Description,
            request.Priority,
            request.Category,
            request.Tags,
            request.ModifiedBy
        );

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}
