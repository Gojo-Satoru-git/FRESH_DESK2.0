using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.Modules.Ticketing.Application.Queries.Tickets;
using Adrenalin.Modules.Ticketing.Application.Queries.Attachments;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, PagedResult<TicketListItemDto>>
{
    private readonly ITicketQueryService _queryService;

    public GetMyTicketsQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<TicketListItemDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetMyTicketsAsync(request.UserId, request.Status, request.Term, request.Page, request.PageSize, cancellationToken);
    }
}

public sealed class GetAssignedTicketsQueryHandler : IRequestHandler<GetAssignedTicketsQuery, PagedResult<TicketListItemDto>>
{
    private readonly ITicketQueryService _queryService;

    public GetAssignedTicketsQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<TicketListItemDto>> Handle(GetAssignedTicketsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetAssignedTicketsAsync(request.AgentId, request.Page, request.PageSize, cancellationToken);
    }
}

public sealed class GetTicketDashboardQueryHandler : IRequestHandler<GetTicketDashboardQuery, TicketDashboardDto>
{
    private readonly ITicketQueryService _queryService;

    public GetTicketDashboardQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<TicketDashboardDto> Handle(GetTicketDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetTicketDashboardAsync(request.CompanyId, request.UserId, cancellationToken);
    }
}

public sealed class SearchTicketsQueryHandler : IRequestHandler<SearchTicketsQuery, PagedResult<TicketListItemDto>>
{
    private readonly ITicketQueryService _queryService;

    public SearchTicketsQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<TicketListItemDto>> Handle(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.SearchTicketsAsync(request, cancellationToken);
    }
}

public sealed class GetTicketActivitiesQueryHandler : IRequestHandler<GetTicketActivitiesQuery, IReadOnlyList<TicketActivityDto>>
{
    private readonly ITicketQueryService _queryService;

    public GetTicketActivitiesQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<IReadOnlyList<TicketActivityDto>> Handle(GetTicketActivitiesQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetTicketActivitiesAsync(request.TicketId, cancellationToken);
    }
}

public sealed class GetTicketCommentsQueryHandler : IRequestHandler<GetTicketCommentsQuery, IReadOnlyList<CommentDto>>
{
    private readonly ITicketQueryService _queryService;

    public GetTicketCommentsQueryHandler(ITicketQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<IReadOnlyList<CommentDto>> Handle(GetTicketCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetTicketCommentsAsync(request.TicketId, request.IncludeInternal, cancellationToken);
    }
}
