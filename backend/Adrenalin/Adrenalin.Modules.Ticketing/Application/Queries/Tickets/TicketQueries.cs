using System;
using System.Collections.Generic;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetMyTicketsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetAssignedTicketsQuery(Guid AgentId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetTicketDashboardQuery(Guid? CompanyId = null, Guid? UserId = null) : IRequest<TicketDashboardDto>;

public sealed record SearchTicketsQuery(
    string? Term,
    string? Status,
    string? Priority,
    string? Category,
    Guid? AssigneeId,
    Guid? ReporterId,
    string? Department,
    string? Region,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetTicketActivitiesQuery(Guid TicketId) : IRequest<IReadOnlyList<TicketActivityDto>>;

public sealed record GetTicketCommentsQuery(Guid TicketId, bool IncludeInternal = false) : IRequest<IReadOnlyList<CommentDto>>;
