using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.Modules.Ticketing.Application.Queries.Groups;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Groups;

public sealed class GetGroupQueueQueryHandler
    : IRequestHandler<GetGroupQueueQuery, Result<GroupQueueSummaryDto>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ITicketRoutingContextRepository _routingContext;

    public GetGroupQueueQueryHandler(
        ITicketRepository ticketRepo,
        ITicketRoutingContextRepository routingContext)
    {
        _ticketRepo = ticketRepo;
        _routingContext = routingContext;
    }

    public async Task<Result<GroupQueueSummaryDto>> Handle(GetGroupQueueQuery query, CancellationToken ct)
    {
        // ── Authorization: caller must be a lead of this group, or a platform admin.
        // Plain agents can't see the whole queue — only their own assigned tickets —
        // dispatching unassigned work is a lead/admin action.
        var isAdmin = await _ticketRepo.IsUserAdminAsync(query.CallerId, ct);
        if (!isAdmin)
        {
            var isLead = await _routingContext.IsUserLeadOfGroupAsync(query.CallerId, query.GroupId, ct);
            if (!isLead)
                return Result<GroupQueueSummaryDto>.Failure(
                    "Only a lead of this group (or an administrator) can view its dispatch queue.");
        }

        var groupName = await _routingContext.GetGroupNameAsync(query.GroupId, ct) ?? "(unknown group)";

        var tickets = await _ticketRepo.GetQueuedTicketsForGroupAsync(query.GroupId, ct);
        var dtos = MapToQueueDtos(tickets, query.GroupId, groupName);

        return Result<GroupQueueSummaryDto>.Success(new GroupQueueSummaryDto(
            query.GroupId,
            groupName,
            dtos.Count,
            dtos.Count == 0 ? 0 : dtos.Max(d => d.WaitingMinutes),
            dtos));
    }

    internal static List<GroupQueueTicketDto> MapToQueueDtos(
        IReadOnlyList<Domain.Entities.Ticket> tickets, Guid groupId, string groupName)
    {
        var now = DateTimeOffset.UtcNow;
        return tickets.Select(t => new GroupQueueTicketDto(
            t.Id,
            t.TicketNumber,
            groupId,
            groupName,
            t.Type.ToString(),
            t.Priority.ToString(),
            t.Status.ToString(),
            t.Title,
            t.CreatedAt,
            (int)(now - t.CreatedAt).TotalMinutes,
            "FactorBased assignment not implemented yet — queued for manual dispatch."
        )).ToList();
    }
}

public sealed class GetMyLeadGroupQueuesQueryHandler
    : IRequestHandler<GetMyLeadGroupQueuesQuery, Result<IReadOnlyList<GroupQueueSummaryDto>>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ITicketRoutingContextRepository _routingContext;

    public GetMyLeadGroupQueuesQueryHandler(
        ITicketRepository ticketRepo,
        ITicketRoutingContextRepository routingContext)
    {
        _ticketRepo = ticketRepo;
        _routingContext = routingContext;
    }

    public async Task<Result<IReadOnlyList<GroupQueueSummaryDto>>> Handle(
        GetMyLeadGroupQueuesQuery query, CancellationToken ct)
    {
        // A user can lead more than one group at once (UserGroup.IsLead is per
        // (UserId, GroupId) row) — this returns one queue summary per group led.
        var ledGroups = await _routingContext.GetGroupsLedByUserAsync(query.CallerId, ct);
        if (ledGroups.Count == 0)
            return Result<IReadOnlyList<GroupQueueSummaryDto>>.Success(Array.Empty<GroupQueueSummaryDto>());

        var groupIds = ledGroups.Select(g => g.GroupId).ToList();
        var allTickets = await _ticketRepo.GetQueuedTicketsForGroupsAsync(groupIds, ct);
        var byGroup = allTickets.GroupBy(t => t.GroupId!.Value).ToDictionary(g => g.Key, g => g.ToList());

        var summaries = ledGroups.Select(g =>
        {
            var groupTickets = byGroup.TryGetValue(g.GroupId, out var list)
                ? list
                : new List<Domain.Entities.Ticket>();

            var dtos = GetGroupQueueQueryHandler.MapToQueueDtos(groupTickets, g.GroupId, g.GroupName);

            return new GroupQueueSummaryDto(
                g.GroupId,
                g.GroupName,
                dtos.Count,
                dtos.Count == 0 ? 0 : dtos.Max(d => d.WaitingMinutes),
                dtos);
        }).ToList();

        return Result<IReadOnlyList<GroupQueueSummaryDto>>.Success(summaries);
    }
}
