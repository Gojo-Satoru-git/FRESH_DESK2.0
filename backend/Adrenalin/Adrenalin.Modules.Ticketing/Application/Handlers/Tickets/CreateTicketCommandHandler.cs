using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDispatcher _dispatcher;
    private readonly ITicketRoutingEngine _routingEngine;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IDispatcher dispatcher,
        ITicketRoutingEngine routingEngine)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
        _routingEngine = routingEngine;
    }

    public async Task<Guid> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticketNumber = await _ticketRepository.GenerateTicketNumberAsync(cancellationToken);

        Guid companyId = Guid.Empty;
        Guid? contactId = null;
        Guid? createdByUserId = null;

        var typeEnum = MapType(request.Type);
        var priorityEnum = MapPriority(request.Priority);

        var (moduleId, moduleName, department) = await _ticketRepository.ResolveOrCreateModuleAsync(request.Type, cancellationToken);
        Console.WriteLine($"createdByUserId: {createdByUserId}");
        Console.WriteLine($"contactId: {contactId}");
        Console.WriteLine($"actorId: {request.ActorId}");
        if (!string.IsNullOrWhiteSpace(request.SenderEmail))
        {
            var email = request.SenderEmail.Trim();
            var resolved = await _ticketRepository.GetContactAndCompanyByEmailAsync(email, cancellationToken);
            if (resolved.HasValue)
            {
                contactId = resolved.Value.ContactId;
                companyId = resolved.Value.CompanyId;
            }
            else
            {
                var emailPrefix = email.Split('@').First();
                var name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(emailPrefix.Replace(".", " ").Replace("_", " "));
                var autoCreated = await _ticketRepository.AutoCreateContactAndCompanyAsync(email, name, cancellationToken);
                contactId = autoCreated.ContactId;
                companyId = autoCreated.CompanyId;
            }
        }
        else if (request.ActorId.HasValue)
        {
            createdByUserId = request.ActorId.Value;
            if (request.IsCustomer)
            {
                var resolved = await _ticketRepository.GetContactAndCompanyByUserIdAsync(request.ActorId.Value, cancellationToken);
                if (resolved.HasValue)
                {
                    contactId = resolved.Value.ContactId;
                    companyId = resolved.Value.CompanyId;
                }
                else
                {
                    var (email, name) = await _ticketRepository.GetUserEmailAndNameAsync(request.ActorId.Value, cancellationToken);
                    var autoCreated = await _ticketRepository.AutoCreateContactForUserAsync(request.ActorId.Value, email, name, cancellationToken);
                    contactId = autoCreated.ContactId;
                    companyId = autoCreated.CompanyId;
                }
            }
            else
            {
                var resolved = await _ticketRepository.GetContactAndCompanyByUserIdAsync(request.ActorId.Value, cancellationToken);
                if (resolved.HasValue)
                {
                    contactId = resolved.Value.ContactId;
                    companyId = resolved.Value.CompanyId;
                }
                else
                {
                    var cmpId = await _ticketRepository.GetUserCompanyIdAsync(request.ActorId.Value, cancellationToken);
                    if (cmpId.HasValue)
                    {
                        companyId = cmpId.Value;
                    }
                    else
                    {
                        var (email, name) = await _ticketRepository.GetUserEmailAndNameAsync(request.ActorId.Value, cancellationToken);
                        var autoCreated = await _ticketRepository.AutoCreateContactForUserAsync(request.ActorId.Value, email, name, cancellationToken);
                        contactId = autoCreated.ContactId;
                        companyId = autoCreated.CompanyId;
                    }
                }
            }
        }

        var source = !string.IsNullOrWhiteSpace(request.SenderEmail) ? TicketSource.Email : TicketSource.Portal;

        var ticket = Ticket.Create(
            companyId: companyId,
            moduleId: moduleId,
            subject: request.Title,
            description: request.Description,
            type: typeEnum,
            source: source,
            createdByUserId: createdByUserId,
            priority: priorityEnum,
            assignedAgentId: null,
            contactId: contactId
        );

        ticket.SetTicketNumber(ticketNumber);

        // ── Enterprise Routing Engine ──────────────────────────────────────
        // Routes the ticket through the 4-tier cascade:
        // 1. Explicit company routing rules
        // 2. Category/Module match
        // 3. Region match
        // 4. Fallback (company default group → system fallback)
        var routingResult = await _routingEngine.RouteAsync(ticket, cancellationToken);
        if (routingResult.GroupId.HasValue)
        {
            ticket.AssignGroup(routingResult.GroupId.Value, createdByUserId ?? Guid.Empty);
        }

        // Enforce Workflow: Tickets must start unassigned so they land in the Lead/Manager queue.
        // Agents cannot see them until explicitly assigned.
        // request.AssigneeId is ignored intentionally.

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //await _dispatcher.Send(
        //    new AssignTicketCommand(
        //        TicketId: ticket.Id,
        //       TriggeredBy: createdByUserId
        //  ?? request.ActorId
        //  ?? Guid.Empty,
        //        IsAutoAssignment: true),
        //    cancellationToken);

        return ticket.Id;
    }

    private TicketType MapType(string type)
    {
        var normalized = type.Trim().ToLowerInvariant();
        if (normalized.Contains("change request") || normalized.Contains("changerequest"))
            return TicketType.ChangeRequest;
        if (normalized.Contains("enhancement") || normalized.Contains("feature request") || normalized.Contains("feature") || normalized.Contains("new requirements"))
            return TicketType.FeatureRequest;
        if (normalized.Contains("bug") || normalized.Contains("software problem") || normalized.Contains("incident") || normalized.Contains("environment issue"))
            return TicketType.Bug;

        return TicketType.Support;
    }

    private TicketPriority MapPriority(string priority)
    {
        var normalized = priority.Trim().ToLowerInvariant();
        if (normalized.Contains("urgent") || normalized.Contains("critical"))
            return TicketPriority.Urgent;
        if (normalized.Contains("high"))
            return TicketPriority.High;
        if (normalized.Contains("low"))
            return TicketPriority.Low;

        return TicketPriority.Medium;
    }
}