using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using System.Text.Json;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class BulkAssignTicketsCommandHandler : IRequestHandler<BulkAssignTicketsCommand, Result<BulkAssignTicketsResult>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ITicketRoutingContextRepository _routingContext;
    private readonly IGroupAssignmentHistoryRepository _historyRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BulkAssignTicketsCommandHandler(
        ITicketRepository ticketRepo,
        ITicketRoutingContextRepository routingContext,
        IGroupAssignmentHistoryRepository historyRepo,
        IUnitOfWork unitOfWork)
    {
        _ticketRepo = ticketRepo;
        _routingContext = routingContext;
        _historyRepo = historyRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BulkAssignTicketsResult>> Handle(BulkAssignTicketsCommand request, CancellationToken cancellationToken)
    {
        if (request.TicketIds == null || request.TicketIds.Count == 0)
        {
            return Result<BulkAssignTicketsResult>.Failure("No ticket IDs provided.");
        }

        if (!request.OverrideAgentId.HasValue && !request.OverrideGroupId.HasValue)
        {
            return Result<BulkAssignTicketsResult>.Failure("Must specify either an agent or a group to assign.");
        }

        // Pre-validate agent membership if both agent and group are provided
        if (request.OverrideAgentId.HasValue && request.OverrideGroupId.HasValue)
        {
            var isMember = await _routingContext.IsUserInGroupAsync(request.OverrideAgentId.Value, request.OverrideGroupId.Value, cancellationToken);
            if (!isMember)
            {
                return Result<BulkAssignTicketsResult>.Failure("Cannot assign agent to tickets in a group they do not belong to.");
            }
        }

        int successfulCount = 0;
        int failedCount = 0;
        var errors = new List<string>();

        var tickets = await _ticketRepo.GetByIdsAsync(request.TicketIds, cancellationToken);
        var ticketDict = tickets.ToDictionary(t => t.Id);
        
        IReadOnlyList<Guid>? overrideAgentGroupIds = null;
        if (request.OverrideAgentId.HasValue)
        {
            overrideAgentGroupIds = await _routingContext.GetUserGroupIdsAsync(request.OverrideAgentId.Value, cancellationToken);
        }

        foreach (var ticketId in request.TicketIds)
        {
            Guid fallbackAgentId = request.OverrideAgentId ?? Guid.Empty; // Required by entity if missing

            if (!ticketDict.TryGetValue(ticketId, out var ticket))
            {
                failedCount++;
                errors.Add($"Ticket {ticketId} not found.");
                
                // Log Failure
                var failLog1 = TicketAssignmentLog.Create(
                    ticketId: ticketId,
                    fromAgentId: null,
                    toAgentId: fallbackAgentId,
                    changedBy: request.TriggeredBy,
                    notes: JsonSerializer.Serialize(new { Source = "BulkAssignment", Result = "Failed", FailureReason = "Ticket not found" })
                );
                await _ticketRepo.AddAssignmentLogAsync(failLog1, cancellationToken);
                continue;
            }

            fallbackAgentId = request.OverrideAgentId ?? ticket.AssignedAgentId ?? Guid.Empty;

            if (ticket.Status == TicketStatus.Closed)
            {
                failedCount++;
                errors.Add($"Ticket {ticketId} is closed and cannot be reassigned.");
                
                var failLog2 = TicketAssignmentLog.Create(
                    ticketId: ticketId,
                    fromAgentId: ticket.AssignedAgentId,
                    toAgentId: fallbackAgentId,
                    changedBy: request.TriggeredBy,
                    notes: JsonSerializer.Serialize(new { Source = "BulkAssignment", Result = "Failed", FailureReason = "Ticket is closed" })
                );
                await _ticketRepo.AddAssignmentLogAsync(failLog2, cancellationToken);
                continue;
            }

            // If we are assigning an agent but keeping the existing group, validate membership
            if (request.OverrideAgentId.HasValue && !request.OverrideGroupId.HasValue && ticket.GroupId.HasValue)
            {
                var isMember = overrideAgentGroupIds != null && overrideAgentGroupIds.Contains(ticket.GroupId.Value);
                if (!isMember)
                {
                    failedCount++;
                    errors.Add($"Ticket {ticketId}: Agent is not a member of the ticket's current group.");
                    
                    var failLog3 = TicketAssignmentLog.Create(
                        ticketId: ticketId,
                        fromAgentId: ticket.AssignedAgentId,
                        toAgentId: fallbackAgentId,
                        changedBy: request.TriggeredBy,
                        notes: JsonSerializer.Serialize(new { Source = "BulkAssignment", Result = "Failed", FailureReason = "Agent not in group" })
                    );
                    await _ticketRepo.AddAssignmentLogAsync(failLog3, cancellationToken);
                    continue;
                }
            }

            Guid? previousGroupId = ticket.GroupId;
            Guid? newGroupId = request.OverrideGroupId ?? previousGroupId;

            var successNotes = JsonSerializer.Serialize(new { Source = "BulkAssignment", Result = "Success", FailureReason = (string?)null });

            await _ticketRepo.UpdateAssignmentAsync(
                ticketId,
                request.OverrideAgentId,
                request.OverrideGroupId, // Only sets the group if provided, wait, UpdateAssignmentAsync sets GroupId to the value provided.
                request.TriggeredBy,
                successNotes,
                clearAgentIfNull: !request.OverrideAgentId.HasValue,
                cancellationToken);

            // Record group assignment history if group changed
            if (request.OverrideGroupId.HasValue && request.OverrideGroupId != previousGroupId)
            {
                _historyRepo.Add(GroupAssignmentHistory.Create(
                    ticketId: ticket.Id,
                    previousGroupId: previousGroupId,
                    newGroupId: request.OverrideGroupId,
                    assignedBy: request.TriggeredBy,
                    reason: "BulkManualAssignment",
                    routingRuleMatched: "ManualAssignment"));
            }

            successfulCount++;
        }

        if (successfulCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<BulkAssignTicketsResult>.Success(new BulkAssignTicketsResult(successfulCount, failedCount, errors));
    }
}
