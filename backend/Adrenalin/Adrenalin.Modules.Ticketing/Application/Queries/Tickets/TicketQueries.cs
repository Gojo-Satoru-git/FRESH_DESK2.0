using System;
using System.Collections.Generic;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Tickets;

public sealed record GetMyTicketsQuery(Guid UserId, string? Status = null, string? Term = null, int Page = 1, int PageSize = 10) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetAssignedTicketsQuery(Guid AgentId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetTicketDashboardQuery(Guid? CompanyId = null, Guid? UserId = null) : IRequest<TicketDashboardDto>;

public sealed record SearchTicketsQuery(
    Guid? GroupId = null,
    Guid? AssignedAgentId = null,
    string? Status = null,
    string? Priority = null,
    bool? Unassigned = null,
    bool? Breached = null,
    string? Search = null,
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null,
    int Page = 1,
    int PageSize = 25
) : IRequest<PagedResult<TicketListItemDto>>;

public sealed record GetTicketActivitiesQuery(Guid TicketId) : IRequest<IReadOnlyList<TicketActivityDto>>;

public sealed record GetTicketCommentsQuery(Guid TicketId, bool IncludeInternal = false) : IRequest<IReadOnlyList<CommentDto>>;
