using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;
using System;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetTicketsQuery
(
    string? TicketNumber,
    string? Status,
    Guid? AssignedAgentId,
    Guid? CompanyId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<TicketListItemDto>>;