using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands.Comments;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Comments;

public sealed class AddEmailReplyCommandHandler : IRequestHandler<AddEmailReplyCommand, Result<Guid>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddEmailReplyCommandHandler> _logger;

    public AddEmailReplyCommandHandler(
        ITicketRepository ticketRepository,
        IEmailMessageRepository emailMessageRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<AddEmailReplyCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _emailMessageRepository = emailMessageRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddEmailReplyCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            return Result<Guid>.Failure($"Ticket '{request.TicketId}' was not found.");
        }

        var emailMessage = await _emailMessageRepository.GetByIdAsync(request.EmailMessageId, cancellationToken);
        if (emailMessage == null)
        {
            return Result<Guid>.Failure($"EmailMessage '{request.EmailMessageId}' not found.");
        }

        var modifiedBy = request.AuthorId ?? request.ContactId
            ?? throw new TicketDomainException("Either AuthorId or ContactId must be provided.");

        var comment = TicketComment.Create(
            request.TicketId,
            request.AuthorId,
            request.ContactId,
            request.PlainTextBody,
            request.HtmlBody,
            request.IsPrivate
        );

        ticket.AddComment(comment, modifiedBy);

        if (request.WatcherUserIds != null && request.WatcherUserIds.Any())
        {
            foreach (var userId in request.WatcherUserIds)
            {
                try
                {
                    ticket.AddWatcher(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Watcher creation failed for UserId {UserId}", userId);
                }
            }
        }

        if (request.Attachments != null && request.Attachments.Any())
        {
            var uploaderId = request.AuthorId ?? request.ContactId ?? Guid.Empty;
            foreach (var attDto in request.Attachments)
            {
                var emailAttachment = emailMessage.Attachments.FirstOrDefault(a => 
                    a.FileName == attDto.FileName && a.Size == attDto.Size);

                string fileUrl = "";
                if (emailAttachment != null && string.IsNullOrEmpty(emailAttachment.StoragePath))
                {
                    using var ms = new MemoryStream(attDto.ContentBytes);
                    var folder = $"email-attachments/{emailMessage.Id}";
                    fileUrl = await _fileStorageService.SaveAsync(ms, attDto.FileName, folder, cancellationToken);
                    emailAttachment.StoragePath = fileUrl;
                    Adrenalin.Modules.Ticketing.Application.Services.EmailMetrics.AttachmentsStored.Add(1);
                }
                else if (emailAttachment != null)
                {
                    fileUrl = emailAttachment.StoragePath!;
                }

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    var ticketAttachment = TicketAttachment.Create(
                        ticket.Id,
                        comment.Id,
                        attDto.FileName,
                        fileUrl,
                        attDto.Size,
                        attDto.ContentType,
                        uploaderId
                    );
                    ticket.AddAttachment(ticketAttachment);

                    if (emailAttachment != null)
                    {
                        emailAttachment.TicketAttachmentId = ticketAttachment.Id;
                    }
                }
            }
        }

        emailMessage.TicketId = ticket.Id;
        emailMessage.TicketCommentId = comment.Id;
        _emailMessageRepository.Update(emailMessage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email reply {CommentId} added to Ticket {TicketId} from EmailMessage {MessageId}", comment.Id, ticket.Id, emailMessage.Id);

        return Result<Guid>.Success(comment.Id);
    }
}
