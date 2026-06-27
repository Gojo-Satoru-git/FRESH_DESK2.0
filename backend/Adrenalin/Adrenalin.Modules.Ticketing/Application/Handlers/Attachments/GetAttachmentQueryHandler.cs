using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;
using Adrenalin.Modules.Ticketing.Application.Queries.Attachments;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class GetAttachmentQueryHandler : IRequestHandler<GetAttachmentQuery, AttachmentStreamResponse?>
{
    private readonly ITicketAttachmentRepository _attachmentRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public GetAttachmentQueryHandler(ITicketAttachmentRepository attachmentRepository, ITicketRepository ticketRepository, IFileStorageService fileStorageService, ICurrentUserService currentUserService)
    {
        _attachmentRepository = attachmentRepository;
        _ticketRepository = ticketRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<AttachmentStreamResponse?> Handle(GetAttachmentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(request.AttachmentId, cancellationToken);

        if (attachment is null || attachment.TicketId != request.TicketId)
        {
            return null;
        }

        var fileExists = await _fileStorageService.ExistsAsync(attachment.FileUrl, cancellationToken);
        if (!fileExists)
        {
            return null;
        }

        var ticket = await _ticketRepository.GetByIdAsync(attachment.TicketId, cancellationToken);
        if (ticket is null)
        {
            return null;
        }
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authorized to download this attachment.");
        }

        var downloaderCompanyId = await _ticketRepository.GetUserCompanyIdAsync(currentUserId.Value, cancellationToken);
        if (downloaderCompanyId != ticket.CompanyId)
        {
            throw new UnauthorizedAccessException("You belong to a different company than the ticket's company.");
        }

        var isAuthorized = false;
        if (currentUserId.HasValue)
        {
            if (attachment.CommentId.HasValue)
            {
                var comment = ticket.TicketComments.FirstOrDefault(c => c.Id == attachment.CommentId.Value);
                if (comment != null && comment.IsPrivate)
                {
                    // Internal comments only accessible by assigned agent (or maybe other agents, but restricted for now)
                    isAuthorized = currentUserId == ticket.AssignedAgentId;
                }
                else
                {
                    isAuthorized = currentUserId == ticket.CreatedByUserId ||
                                   currentUserId == ticket.AssignedAgentId;
                }
            }
            else
            {
                isAuthorized = currentUserId == ticket.CreatedByUserId ||
                               currentUserId == ticket.AssignedAgentId;
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to download this attachment.");
        }

        var stream = await _fileStorageService.OpenReadAsync(attachment.FileUrl, cancellationToken);

        return new AttachmentStreamResponse(stream, attachment.FileName, attachment.MimeType);
    }
}
