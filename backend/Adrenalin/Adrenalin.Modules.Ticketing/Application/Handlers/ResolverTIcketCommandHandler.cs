using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Persistence.Entities;
using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public class ResolveTicketCommandHandler
    : IRequestHandler<ResolveTicketCommand, Result<bool>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveTicketCommandHandler(
        ITicketRepository ticketRepo,
        IUnitOfWork unitOfWork)
    {
        _ticketRepo = ticketRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        ResolveTicketCommand command, CancellationToken ct)
    {
        // 1. Load the ticket
        var ticket = await _ticketRepo.GetByIdAsync(command.TicketId, ct);
        if (ticket is null)
            return Result<bool>.Failure("Ticket not found");

        // 2. Get current status from history
        var currentStatus = await _ticketRepo
            .GetCurrentStatusAsync(command.TicketId, ct);

        // 3. Validate transition
        if (currentStatus == "Resolved" || currentStatus == "Closed")
            return Result<bool>.Failure($"Ticket is already {currentStatus}");

        // 4. Check graph allows this transition
        var transitionAllowed = ticket.Graph?.StatusTransitions
            .Any(t => t.FromStatus == currentStatus
                   && t.ToStatus == "Resolved") ?? true;

        if (!transitionAllowed)
            return Result<bool>.Failure(
                $"Transition from {currentStatus} to Resolved is not allowed");

        // 5. INSERT into TicketStatusHistory
        var history = new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            FromStatus = currentStatus,
            ToStatus = "Resolved",
            ChangedBy = command.AgentId,
            ChangedAt = DateTime.UtcNow,
            Reason = command.ResolutionNote
        };

        await _ticketRepo.AddStatusHistoryAsync(history, ct);

        // 6. Update Ticket metadata
        ticket.Rca = command.ResolutionNote;
        ticket.UpdatedBy = command.AgentId;
        ticket.UpdatedAt = DateTime.UtcNow;

        // 7. Commit
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}