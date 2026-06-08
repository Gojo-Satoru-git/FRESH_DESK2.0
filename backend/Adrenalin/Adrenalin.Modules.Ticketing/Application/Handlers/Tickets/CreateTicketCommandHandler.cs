// ✅ CORRECT — usings before everything
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDispatcher _dispatcher;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IDispatcher dispatcher)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
    }

    public async Task<Guid> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(
            companyId: request.CompanyId,
            moduleId: request.ModuleId,
            subject: request.Subject,
            description: request.Description,
            createdByUserId: request.CreatedByUserId
        );

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _dispatcher.Send(
            new AssignTicketCommand(
                TicketId: ticket.Id,
                TriggeredBy: request.CreatedByUserId
                                  ?? Guid.Empty,
                IsAutoAssignment: true),
            cancellationToken);

        return ticket.Id;
    }
}