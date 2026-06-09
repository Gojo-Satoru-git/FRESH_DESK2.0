using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class DeleteTicketAttachmentCommandHandler : IRequestHandler<DeleteTicketAttachmentCommand, Guid>
{
    private readonly ITicketAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTicketAttachmentCommandHandler(ITicketAttachmentRepository attachmentRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteTicketAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(request.AttachmentId, cancellationToken);

        if (attachment is null || attachment.TicketId != request.TicketId)
        {
            throw new TicketDomainException($"Attachment '{request.AttachmentId}' not found for ticket '{request.TicketId}'.");
        }

        _attachmentRepository.Remove(attachment);

        await _fileStorageService.DeleteAsync(attachment.FileUrl, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }
}
