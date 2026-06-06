using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(
            companyId: request.CompanyId,
            moduleId: request.ModuleId,
            subject: request.Subject,
            description: request.Description,
            createdByUserId: request.CreatedByUserId
        );

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        return ticket.Id;
    }
}