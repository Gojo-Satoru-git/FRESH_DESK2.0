using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetTicketHistoryQueryHandler : IRequestHandler<GetTicketHistoryQuery, TicketHistoryDto>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketHistoryQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketHistoryDto> Handle(GetTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var statusHistory = ticket.TicketStatusHistories
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new StatusHistoryDto(
                x.Id,
                x.FromStatus?.ToString(),
                x.ToStatus.ToString(),
                x.ChangedBy,
                x.Reason,
                x.ChangedAt
            ))
            .ToList();

        var assignmentLogs = ticket.TicketAssignmentLogs
            .OrderByDescending(x => x.AssignedAt)
            .Select(x => new AssignmentLogDto(
                x.Id,
                x.FromAgentId,
                x.ToAgentId,
                x.ChangedBy,
                x.Notes,
                x.AssignedAt
            ))
            .ToList();

        return new TicketHistoryDto(statusHistory, assignmentLogs);
    }
}
