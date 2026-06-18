using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class ClaimTicketCommandHandler : IRequestHandler<ClaimTicketCommand, Result<Guid>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ITicketRoutingContextRepository _routingContext;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimTicketCommandHandler(
        ITicketRepository ticketRepo,
        ITicketRoutingContextRepository routingContext,
        IUnitOfWork unitOfWork)
    {
        _ticketRepo = ticketRepo;
        _routingContext = routingContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ClaimTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
            return Result<Guid>.Failure($"Ticket {request.TicketId} not found");

        if (ticket.AssignedAgentId.HasValue)
            return Result<Guid>.Failure("Ticket is already assigned to an agent.");

        if (ticket.GroupId.HasValue)
        {
            var isMember = await _routingContext.IsUserInGroupAsync(request.AgentId, ticket.GroupId.Value, cancellationToken);
            if (!isMember)
            {
                return Result<Guid>.Failure("Agent is not a member of the group this ticket is assigned to.");
            }
        }

        if (ticket.RowVersion == null)
            return Result<Guid>.Failure("Ticket is missing concurrency token.");

        var claimed = await _ticketRepo.TryClaimTicketAsync(
            ticket.Id,
            request.AgentId,
            ticket.RowVersion,
            cancellationToken);

        if (!claimed)
        {
            return Result<Guid>.Failure("Ticket was already claimed or modified by another user. Please refresh and try again.");
        }

        // Add the assignment log since we bypassed the normal UpdateAssignmentAsync
        var log = Adrenalin.Modules.Ticketing.Domain.Entities.TicketAssignmentLog.Create(
            ticket.Id, null, request.AgentId, request.AgentId, "Agent self-assigned ticket");
        
        await _ticketRepo.AddAssignmentLogAsync(log, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(ticket.Id);
    }
}
