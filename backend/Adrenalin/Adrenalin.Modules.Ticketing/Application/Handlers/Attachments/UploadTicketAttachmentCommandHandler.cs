using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands.Attachments;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class UploadTicketAttachmentCommandHandler : IRequestHandler<UploadTicketAttachmentCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadTicketAttachmentCommandHandler(ITicketRepository ticketRepository, ITicketAttachmentRepository ticketAttachmentRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _attachmentRepository = ticketAttachmentRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(UploadTicketAttachmentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new TicketDomainException($"Ticket '{request.TicketId}' not found.");
        }

        if (request.CommentId.HasValue)
        {
            var commentExists = ticket.TicketComments.Any(c => c.Id == request.CommentId.Value);
            if (!commentExists)
            {
                throw new TicketDomainException($"Comment '{request.CommentId.Value}' does not belong to ticket '{request.TicketId}'.");
            }
        }

        if (!string.IsNullOrEmpty(request.FileName))
        {
            var extension = Path.GetExtension(request.FileName);
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xlsx", ".csv", ".png", ".jpg", ".jpeg", ".txt", ".zip", ".mp4" , ".msg" , ".eml" };
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new TicketDomainException($"File extension '{extension}' is not allowed.");
            }
        }

        var uploadedByCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.UploadedBy, cancellationToken);
        if (uploadedByCompanyId != ticket.CompanyId)
        {
            throw new TicketDomainException("UploadedBy user must belong to the same company as the ticket.");
        }

        var fileUrl = await _fileStorageService.SaveAsync(request.Stream, request.FileName, "attachments", cancellationToken);

        var attachment = TicketAttachment.Create(
            request.TicketId, 
            request.CommentId, 
            request.FileName, 
            fileUrl, 
            request.Length, 
            request.ContentType, 
            request.UploadedBy);

        await _attachmentRepository.AddAsync(attachment, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }
}