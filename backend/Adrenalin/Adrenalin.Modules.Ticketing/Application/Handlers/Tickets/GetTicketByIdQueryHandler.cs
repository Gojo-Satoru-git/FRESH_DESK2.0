using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;
using Adrenalin.Modules.Ticketing.Application.Queries.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Enums;

using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, GetTicketByIdResponse>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetTicketByIdResponse> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var isInternal = _currentUserService.Roles.Intersect(new[] { "admin", "manager", "pmo", "team_lead", "senior_agent", "junior_agent" }, StringComparer.OrdinalIgnoreCase).Any();

        var comments = (request.IncludeInternalComments && isInternal)
            ? ticket.TicketComments
            : ticket.TicketComments
                .Where(x => !x.IsPrivate);

        // Collect all referenced user IDs and contact IDs to resolve in batch
        var userIds = new List<Guid>();
        if (ticket.CreatedByUserId.HasValue) userIds.Add(ticket.CreatedByUserId.Value);
        if (ticket.AssignedAgentId.HasValue) userIds.Add(ticket.AssignedAgentId.Value);
        
        foreach (var comment in ticket.TicketComments)
        {
            if (comment.AuthorId.HasValue) userIds.Add(comment.AuthorId.Value);
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

        var statusString = ticket.Status.ToString();
        if (!isInternal && new[] { TicketStatus.PendingInternal, TicketStatus.OnHold, TicketStatus.ProductRoadmap, TicketStatus.PendingApproval, TicketStatus.ComplianceReview, TicketStatus.DualAgentConfirm }.Contains(ticket.Status))
        {
            statusString = "InProgress";
        }

        var internalStatuses = new[] { TicketStatus.PendingInternal, TicketStatus.OnHold, TicketStatus.ProductRoadmap, TicketStatus.PendingApproval, TicketStatus.ComplianceReview, TicketStatus.DualAgentConfirm };
        
        var mappedHistory = new List<StatusHistoryDto>();
        if (isInternal)
        {
            mappedHistory = ticket.TicketStatusHistories
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
        }
        else
        {
            var chronologicalHistory = ticket.TicketStatusHistories.OrderBy(x => x.ChangedAt).ToList();
            string? lastMappedToStatus = null;
            
            foreach (var history in chronologicalHistory)
            {
                var mappedFromStatus = history.FromStatus.HasValue 
                    ? (internalStatuses.Contains(history.FromStatus.Value) ? "InProgress" : history.FromStatus.Value.ToString()) 
                    : null;
                    
                var mappedToStatus = internalStatuses.Contains(history.ToStatus) ? "InProgress" : history.ToStatus.ToString();
                
                if (mappedToStatus == lastMappedToStatus)
                {
                    continue; // Skip consecutive duplicates for customer view
                }
                
                mappedHistory.Add(new StatusHistoryDto(
                    history.Id, 
                    mappedFromStatus, 
                    mappedToStatus, 
                    history.ChangedBy, 
                    history.Reason, 
                    history.ChangedAt
                ));
                lastMappedToStatus = mappedToStatus;
            }
            mappedHistory.Reverse(); // Return newest first to match OrderByDescending
        }

        return new GetTicketByIdResponse(
            Id:             ticket.Id,
            TicketNumber:   ticket.TicketNumber,
            Title:          ticket.Title,
            Description:    ticket.Description,
            Status:         statusString,
            Priority:       ticket.Priority.ToString(),
            Type:           ticket.Type.ToString(),
            AssignedAgentId: isInternal ? ticket.AssignedAgentId : null,
            ReporterId:     ticket.CreatedByUserId,
            CompanyId:      ticket.CompanyId,
            CreatedAt:      ticket.CreatedAt,
            UpdatedAt:      ticket.UpdatedAt,

            Comments: comments
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CommentDto(
                    x.Id,
                    x.AuthorId,
                    x.ContactId,
                    x.Body,
                    x.IsPrivate ? "Internal" : "Public",
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

            StatusHistory: mappedHistory,

            AssignmentLogs: isInternal ? ticket.TicketAssignmentLogs
                .OrderByDescending(x => x.AssignedAt)
                .Select(x => new AssignmentLogDto(
                    x.Id,
                    x.FromAgentId,
                    x.ToAgentId,
                    x.ChangedBy,
                    x.Notes,
                    x.AssignedAt
                ))
                .ToList() : new List<AssignmentLogDto>(),

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

            ReporterName: reporterName,
            AssignedAgentName: isInternal ? assignedAgentName : null
        );
    }
}