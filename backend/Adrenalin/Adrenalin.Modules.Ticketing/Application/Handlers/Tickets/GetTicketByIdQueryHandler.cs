using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;

using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, GetTicketByIdResponse>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetTicketByIdResponse> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var comments = request.IncludeInternalComments
            ? ticket.TicketComments
            : ticket.TicketComments
                .Where(x => x.Visibility != Adrenalin.Modules.Ticketing.Domain.Enums.CommentVisibility.Internal);

        // Collect all referenced user IDs and contact IDs to resolve in batch
        var userIds = new List<Guid>();
        if (ticket.CreatedByUserId.HasValue) userIds.Add(ticket.CreatedByUserId.Value);
        if (ticket.AssignedAgentId.HasValue) userIds.Add(ticket.AssignedAgentId.Value);
        
        foreach (var comment in ticket.TicketComments)
        {
            if (comment.AuthorId.HasValue) userIds.Add(comment.AuthorId.Value);
        }
        
        foreach (var activity in ticket.TicketActivities)
        {
            if (activity.PerformedBy.HasValue) userIds.Add(activity.PerformedBy.Value);
        }

        var contactIds = ticket.TicketComments
            .Where(c => c.ContactId.HasValue)
            .Select(c => c.ContactId!.Value)
            .ToList();

        var contactDisplayNames = await _ticketRepository.GetContactDisplayNamesAsync(contactIds, cancellationToken);

        var unresolvedContactIds = contactIds.Where(id => !contactDisplayNames.ContainsKey(id)).ToList();
        if (unresolvedContactIds.Any())
        {
            userIds.AddRange(unresolvedContactIds);
        }

        var userDisplayNames = await _ticketRepository.GetUserDisplayNamesAsync(userIds, cancellationToken);

        foreach (var contactId in unresolvedContactIds)
        {
            if (userDisplayNames.TryGetValue(contactId, out var userName))
            {
                contactDisplayNames[contactId] = userName;
            }
        }

        string? reporterName = ticket.CreatedByUserId.HasValue && userDisplayNames.TryGetValue(ticket.CreatedByUserId.Value, out var rn) ? rn : null;
        string? assignedAgentName = ticket.AssignedAgentId.HasValue && userDisplayNames.TryGetValue(ticket.AssignedAgentId.Value, out var an) ? an : null;

        return new GetTicketByIdResponse(
            Id:             ticket.Id,
            TicketNumber:   ticket.TicketNumber,
            Title:          ticket.Title,
            Description:    ticket.Description,
            Status:         ticket.Status.ToString(),
            Priority:       ticket.Priority.ToString(),
            Category:       ticket.Category.ToString(),
            AssignedAgentId: ticket.AssignedAgentId,
            ReporterId:     ticket.CreatedByUserId,
            CompanyId:      ticket.CompanyId,
            Department:     ticket.Department,
            Region:         ticket.Region,
            ModuleName:     ticket.ModuleName,
            CreatedAt:      ticket.CreatedAt,
            UpdatedAt:      ticket.UpdatedAt,
            ResolvedAt:     ticket.ResolvedAt,
            ClosedAt:       ticket.ClosedAt,

            Tags: ticket.TicketTags
                .Select(t => t.TagName)
                .ToList(),

            Comments: comments
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CommentDto(
                    x.Id,
                    x.AuthorId,
                    x.ContactId,
                    x.Body,
                    x.Visibility.ToString(),
                    x.CreatedAt,
                    Attachments: x.Attachments
                        .Select(a => new AttachmentDto(
                            a.Id,
                            a.FileName,
                            a.FileUrl,
                            a.FileSizeBytes,
                            a.MimeType,
                            a.CreatedAt
                        ))
                        .ToList(),
                    AuthorName: x.AuthorId.HasValue && userDisplayNames.TryGetValue(x.AuthorId.Value, out var aun) ? aun : null,
                    ContactName: x.ContactId.HasValue && contactDisplayNames.TryGetValue(x.ContactId.Value, out var cn) ? cn : null
                ))
                .ToList(),

            StatusHistory: ticket.TicketStatusHistories
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new StatusHistoryDto(
                    x.Id,
                    x.FromStatus?.ToString(),
                    x.ToStatus.ToString(),
                    x.ChangedBy,
                    x.Reason,
                    x.ChangedAt
                ))
                .ToList(),

            AssignmentLogs: ticket.TicketAssignmentLogs
                .OrderByDescending(x => x.AssignedAt)
                .Select(x => new AssignmentLogDto(
                    x.Id,
                    x.FromAgentId,
                    x.ToAgentId,
                    x.ChangedBy,
                    x.Notes,
                    x.AssignedAt
                ))
                .ToList(),

            Watchers: ticket.TicketWatchers
                .OrderByDescending(x => x.AddedAt)
                .Select(x => new WatcherDto(
                    x.Id,
                    x.UserId,
                    x.AddedAt
                ))
                .ToList(),

            Relations: ticket.TicketRelations
                .OrderByDescending(x => x.Id)
                .Select(x => new RelationDto(
                    x.Id,
                    x.ParentTicketId,
                    x.ChildTicketId,
                    x.RelationType.ToString()
                ))
                .ToList(),

            Attachments: ticket.TicketAttachments
                .Where(x => x.CommentId == null)
                .Select(a => new AttachmentDto(
                    a.Id,
                    a.FileName,
                    a.FileUrl,
                    a.FileSizeBytes,
                    a.MimeType,
                    a.CreatedAt
                ))
                .ToList(),

            Activities: ticket.TicketActivities
                .OrderByDescending(x => x.PerformedAt)
                .Select(x => new TicketActivityDto(
                    x.Id,
                    x.ActivityType,
                    x.OldValue,
                    x.NewValue,
                    x.PerformedBy,
                    x.PerformedAt,
                    PerformedByName: x.PerformedBy.HasValue && userDisplayNames.TryGetValue(x.PerformedBy.Value, out var pbn) ? pbn : null
                ))
                .ToList(),

            ReporterName: reporterName,
            AssignedAgentName: assignedAgentName
        );
    }
}