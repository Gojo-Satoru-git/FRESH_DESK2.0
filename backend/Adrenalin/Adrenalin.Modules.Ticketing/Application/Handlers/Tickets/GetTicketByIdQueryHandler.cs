using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;

using Adrenalin.SharedKernel.Mediator;

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

        if(ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' was not found.");
        }

        var comments = request.InclueInternalComments 
            ? ticket.TicketComments 
            : ticket.TicketComments
            .Where(x => x.Visibility != Adrenalin.Modules.Ticketing.Domain.Enums.CommentVisibility.Internal);

        return new GetTicketByIdResponse
        (
            ticket.Id,
            ticket.TicketNumber,
            ticket.Subject,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.AssignedAgentId,
            ticket.CompanyId,
            ticket.CreatedAt,

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
                        .ToList()
                ))
                .ToList(),

            StatusHistory: ticket.TicketStatusHistories
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new StatusHistoryDto
                (
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
                .Select(x => new AssignmentLogDto
                (   
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
                .Select(x => new WatcherDto
                (
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
                .ToList()
        );
    }
}