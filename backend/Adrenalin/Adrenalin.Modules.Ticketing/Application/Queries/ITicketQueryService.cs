using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.Modules.Ticketing.Application.Queries.Tickets;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public interface ITicketQueryService
{
    Task<PagedResult<TicketListItemDto>> GetMyTicketsAsync(Guid userId, string? status, string? term, int page, int pageSize, CancellationToken cancellationToken);
    
    Task<PagedResult<TicketListItemDto>> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize, CancellationToken cancellationToken);
    
    Task<TicketDashboardDto> GetTicketDashboardAsync(Guid? companyId, Guid? userId, CancellationToken cancellationToken);
    
    Task<PagedResult<TicketListItemDto>> SearchTicketsAsync(SearchTicketsQuery query, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<TicketActivityDto>> GetTicketActivitiesAsync(Guid ticketId, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<CommentDto>> GetTicketCommentsAsync(Guid ticketId, bool includeInternal, CancellationToken cancellationToken);
}
