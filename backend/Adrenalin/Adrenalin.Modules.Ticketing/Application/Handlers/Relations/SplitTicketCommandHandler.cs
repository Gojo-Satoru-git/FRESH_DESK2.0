using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class SplitTicketCommandHandler : IRequestHandler<SplitTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public SplitTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(SplitTicketCommand request, CancellationToken cancellationToken)
    {
        var originalTicket = await _ticketRepository.GetByIdAsync(request.OriginalTicketId, cancellationToken);
        if (originalTicket is null)
        {
            throw new TicketDomainException($"Original ticket '{request.OriginalTicketId}' was not found.");
        }

        if (originalTicket.Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Cannot split a closed ticket.");
        }

        var creatorCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.CreatedByUserId, cancellationToken);
        
        if (creatorCompanyId != originalTicket.CompanyId)
        {
            throw new TicketDomainException("The user splitting the ticket must belong to the same company as the original ticket.");
        }

        if (request.CommentIdsToMove is not null)
        {
            foreach (var commentId in request.CommentIdsToMove)
            {
                if (!originalTicket.TicketComments.Any(c => c.Id == commentId))
                {
                    throw new TicketDomainException($"Comment '{commentId}' does not belong to the original ticket.");
                }
            }
        }

        if (request.AttachmentIdsToMove is not null)
        {
            foreach (var attachmentId in request.AttachmentIdsToMove)
            {
                if (!originalTicket.TicketAttachments.Any(a => a.Id == attachmentId))
                {
                    throw new TicketDomainException($"Attachment '{attachmentId}' does not belong to the original ticket.");
                }
            }
        }

        var newTicket = Ticket.Create(
            companyId: originalTicket.CompanyId,
            moduleId: originalTicket.ModuleId,
            subject: request.NewSubject,
            description: request.NewDescription,
            createdByUserId: request.CreatedByUserId
        );

        await _ticketRepository.AddAsync(newTicket, cancellationToken);

        var relation = TicketRelation.Create(
            originalTicket.Id,
            newTicket.Id,
            TicketRelationType.SplitFrom
        );
        originalTicket.AddRelation(relation);

        if (request.CommentIdsToMove is not null)
        {
            foreach (var commentId in request.CommentIdsToMove)
            {
                originalTicket.MoveComment(commentId, newTicket);
            }
        }

        if (request.AttachmentIdsToMove is not null)
        {
            foreach (var attachmentId in request.AttachmentIdsToMove)
            {
                originalTicket.MoveAttachment(attachmentId, newTicket);
            }
        }

        var originalCommentBody = $"Ticket split. Created new ticket {newTicket.TicketNumber ?? newTicket.Id.ToString()}.";
        
        var originalComment = TicketComment.Create(
            originalTicket.Id,
            request.CreatedByUserId,
            null,
            originalCommentBody,
            CommentVisibility.Internal
        );
        originalTicket.AddComment(originalComment, request.CreatedByUserId);

        var newCommentBody = $"Ticket split from original ticket {originalTicket.TicketNumber ?? originalTicket.Id.ToString()}.";
        
        var newComment = TicketComment.Create(
            newTicket.Id,
            request.CreatedByUserId,
            null,
            newCommentBody,
            CommentVisibility.Internal
        );
        newTicket.AddComment(newComment, request.CreatedByUserId);

        return newTicket.Id;
    }
}
