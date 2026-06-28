using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.EventBus;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IWorkflowValidator _workflowValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;

    public ChangeTicketStatusCommandHandler(
        ITicketRepository ticketRepository, 
        IWorkflowValidator workflowValidator, 
        ICurrentUserService currentUserService,
        IEventBus eventBus)
    {
        _ticketRepository = ticketRepository;
        _workflowValidator = workflowValidator;
        _currentUserService = currentUserService;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if(ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' not found.");
        }

        var roles = _currentUserService.Roles.ToList();
        var isAgent = roles.Contains("junior_agent", StringComparer.OrdinalIgnoreCase) || 
                      roles.Contains("senior_agent", StringComparer.OrdinalIgnoreCase);
        var isTeamLead = roles.Contains("team_lead", StringComparer.OrdinalIgnoreCase);
        var isCustomer = roles.Contains("customer", StringComparer.OrdinalIgnoreCase);

        if (ticket.GraphId.HasValue)
        {
            // Team leads are allowed to directly resolve tickets, bypassing workflow
            bool bypassWorkflow = isTeamLead && request.NewStatus == TicketStatus.Resolved;

            if (!bypassWorkflow)
            {
                var isAllowed = await _workflowValidator.IsTransitionAllowedAsync(
                    ticket.GraphId.Value,
                    ticket.Status.ToString(),
                    request.NewStatus.ToString(),
                    _currentUserService.Roles,
                    cancellationToken);

                if (!isAllowed)
                {
                    throw new TicketDomainException($"State transition from '{ticket.Status}' to '{request.NewStatus}' is not allowed or you do not have the required role.");
                }
            }
        }

        if (isAgent && ticket.AssignedAgentId != request.ChangedBy)
        {
            throw new TicketDomainException("Agents can only modify the status of tickets assigned to them.");
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
                if (!isCustomer)
                {
                    throw new TicketDomainException("Only customers can reopen tickets.");
                }
                ticket.Reopen(request.ChangedBy, request.Reason ?? string.Empty);
                break;
            default:
                ticket.ChangeStatus(request.NewStatus, request.ChangedBy, request.Reason);
                break;
        }

        _ticketRepository.Update(ticket);

        return ticket.Id;
    }
}