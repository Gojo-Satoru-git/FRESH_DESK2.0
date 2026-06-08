using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticketNumber = await _ticketRepository.GenerateTicketNumberAsync(cancellationToken);

        Guid companyId = Guid.Empty;
        Guid? contactId = null;
        Guid? createdByUserId = null;

        // 1. Resolve Category and Priority
        var categoryEnum = MapCategory(request.Category);
        var priorityEnum = MapPriority(request.Priority);

        // 2. Resolve Module, ModuleName, and Department
        var (moduleId, moduleName, department) = await _ticketRepository.ResolveOrCreateModuleAsync(request.Category, cancellationToken);

        // 3. Resolve Contact & Company
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

        var region = await _ticketRepository.GetCompanyRegionAsync(companyId, cancellationToken);

        var ticket = Ticket.Create(
            companyId: companyId,
            moduleId: moduleId,
            subject: request.Title,
            description: request.Description,
            createdByUserId: createdByUserId,
            category: categoryEnum,
            priority: priorityEnum,
            moduleName: !string.IsNullOrWhiteSpace(request.ModuleName) ? request.ModuleName : moduleName,
            assignedAgentId: null,
            department: department,
            region: region,
            contactId: contactId
        );

        ticket.SetTicketNumber(ticketNumber);

        var actor = createdByUserId ?? contactId ?? Guid.Empty;

        // Transition from New to Open immediately upon creation
        ticket.ChangeStatus(TicketStatus.Open, actor, "Ticket Created");

        // Assign agent if provided
        if (request.AssigneeId.HasValue && request.AssigneeId.Value != Guid.Empty)
        {
            ticket.AssignAgent(request.AssigneeId.Value, actor, "Assigned upon creation");
        }

        if (request.Tags != null)
        {
            foreach (var tag in request.Tags)
            {
                ticket.AddTag(tag, actor);
            }
        }

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        return ticket.Id;
    }

    private TicketCategory MapCategory(string category)
    {
        var normalized = category.Trim().ToLowerInvariant();
        if (normalized.Contains("change request") || normalized.Contains("changerequest"))
            return TicketCategory.ChangeRequest;
        if (normalized.Contains("enhancement") || normalized.Contains("feature request") || normalized.Contains("feature") || normalized.Contains("new requirements"))
            return TicketCategory.FeatureRequest;
        if (normalized.Contains("bug") || normalized.Contains("software problem") || normalized.Contains("incident") || normalized.Contains("environment issue"))
            return TicketCategory.Bug;

        return TicketCategory.Support;
    }

    private TicketPriority MapPriority(string priority)
    {
        var normalized = priority.Trim().ToLowerInvariant();
        if (normalized.Contains("urgent") || normalized.Contains("critical"))
            return TicketPriority.Critical;
        if (normalized.Contains("high"))
            return TicketPriority.High;
        if (normalized.Contains("low"))
            return TicketPriority.Low;

        return TicketPriority.Medium;
    }
}