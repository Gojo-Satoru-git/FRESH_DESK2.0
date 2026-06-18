using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class CreateTicketFromEmailCommandHandler : IRequestHandler<CreateTicketFromEmailCommand, Result<Guid>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDispatcher _dispatcher;
    private readonly ITicketRoutingEngine _routingEngine;
    private readonly ILogger<CreateTicketFromEmailCommandHandler> _logger;

    public CreateTicketFromEmailCommandHandler(
        ITicketRepository ticketRepository,
        IEmailMessageRepository emailMessageRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        IDispatcher dispatcher,
        ITicketRoutingEngine routingEngine,
        ILogger<CreateTicketFromEmailCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _emailMessageRepository = emailMessageRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
        _routingEngine = routingEngine;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTicketFromEmailCommand request, CancellationToken cancellationToken)
    {
        var emailMessage = await _emailMessageRepository.GetByIdAsync(request.EmailMessageId, cancellationToken);
        if (emailMessage == null)
        {
            return Result<Guid>.Failure($"EmailMessage {request.EmailMessageId} not found.");
        }

        var ticketNumber = await _ticketRepository.GenerateTicketNumberAsync(cancellationToken);

        var ticket = Ticket.Create(
            companyId: request.CompanyId,
            moduleId: request.ModuleId,
            subject: string.IsNullOrWhiteSpace(request.Subject) ? "(No Subject)" : request.Subject,
            description: !string.IsNullOrWhiteSpace(request.PlainTextBody) ? request.PlainTextBody :
                         !string.IsNullOrWhiteSpace(request.HtmlBody) ? request.HtmlBody : "(No Description)",
            type: TicketType.Incident,
            source: TicketSource.Email,
            createdByUserId: request.CreatedByUserId,
            priority: TicketPriority.Medium,
            assignedAgentId: null,
            contactId: request.ContactId
        );

        ticket.SetTicketNumber(ticketNumber);

        // Add watchers from extracted CC/To emails
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

        var routingResult = await _routingEngine.RouteAsync(ticket, cancellationToken);
        if (routingResult.GroupId.HasValue)
        {
            ticket.AssignGroup(routingResult.GroupId.Value, request.CreatedByUserId ?? Guid.Empty);
        }

        if (request.Attachments != null && request.Attachments.Any())
        {
            var uploaderId = request.CreatedByUserId; // Can be null for system or external contact
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
                        null,
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

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        
        emailMessage.TicketId = ticket.Id;
        _emailMessageRepository.Update(emailMessage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _dispatcher.Send(
            new AssignTicketCommand(
                TicketId: ticket.Id,
                TriggeredBy: request.CreatedByUserId ?? Guid.Empty,
                IsAutoAssignment: true),
            cancellationToken);

        _logger.LogInformation("Ticket {TicketId} created from EmailMessage {MessageId}", ticket.Id, emailMessage.Id);

        return Result<Guid>.Success(ticket.Id);
    }
}
