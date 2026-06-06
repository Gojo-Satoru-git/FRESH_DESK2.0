using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PagedResult<TicketListItemDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<PagedResult<TicketListItemDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        TicketStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<TicketStatus>(request.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        var tickets =
            await _ticketRepository.GetTicketsAsync(
                request.TicketNumber,
                status,
                request.AssignedAgentId,
                request.CompanyId,
                request.Page,
                request.PageSize,
                cancellationToken);

        var totalCount =
            await _ticketRepository.CountTicketsAsync(
                request.TicketNumber,
                status,
                request.AssignedAgentId,
                request.CompanyId,
                cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,

            Items = tickets
                .Select(x => new TicketListItemDto
                (
                    x.Id,
                    x.TicketNumber ?? string.Empty,
                    x.Subject,
                    x.Status.ToString(),
                    x.AssignedAgentId,
                    x.CompanyId,
                    x.CreatedAt
                ))
                .ToList()
        };
    }
}