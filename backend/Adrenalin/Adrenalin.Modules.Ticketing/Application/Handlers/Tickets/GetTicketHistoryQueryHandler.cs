using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.Queries.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetTicketHistoryQueryHandler : IRequestHandler<GetTicketHistoryQuery, TicketHistoryDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly Adrenalin.SharedKernel.Interfaces.ICurrentUserService _currentUserService;

    public GetTicketHistoryQueryHandler(ITicketRepository ticketRepository, Adrenalin.SharedKernel.Interfaces.ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TicketHistoryDto> Handle(GetTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var isInternal = _currentUserService.Roles.Intersect(new[] { "admin", "manager", "pmo", "team_lead", "senior_agent", "junior_agent" }, StringComparer.OrdinalIgnoreCase).Any();

        var statusHistory = isInternal 
            ? ticket.TicketStatusHistories
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new StatusHistoryDto(x.Id, x.FromStatus?.ToString(), x.ToStatus.ToString(), x.ChangedBy, x.Reason, x.ChangedAt))
                .ToList()
            : ticket.TicketStatusHistories
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new StatusHistoryDto(x.Id, x.FromStatus?.ToString(), x.ToStatus.ToString(), Guid.Empty, x.Reason, x.ChangedAt))
                .ToList();

        var assignmentLogs = isInternal
            ? ticket.TicketAssignmentLogs
                .OrderByDescending(x => x.AssignedAt)
                .Select(x => new AssignmentLogDto(x.Id, x.FromAgentId, x.ToAgentId, x.ChangedBy, x.Notes, x.AssignedAt))
                .ToList()
            : new List<AssignmentLogDto>();

        return new TicketHistoryDto(statusHistory, assignmentLogs);
    }
}
