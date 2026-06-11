using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Events;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Mediator;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class Ticket : SoftDeleteEntity
{
    public string? TicketNumber { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? ContactId { get; private set; } 
    public Guid? GroupId { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? GraphId { get; private set; }
    public Guid? VersionId { get; private set; }
    public Guid ModuleId { get; private set; }
    public Guid? SubModuleId { get; private set; }
    
    public string Title { get; private set; } = null!; // Maps to 'subject'
    public string Description { get; private set; } = null!;
    
    public TicketSource Source { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; } = TicketStatus.New;
    public TicketType Type { get; private set; }

    public string? ProductType { get; private set; }
    public string? SolutionType { get; private set; }
    public string? FixType { get; private set; }
    public string? Rca { get; private set; }
    public bool CustomerCallTaken { get; private set; }
    public string? LinkedJiraId { get; private set; }
    // public string? AiRating { get; private set; } // Removing because ai_rating uses custom agent_rating enum and is handled separately or ignored for now if unused
    public Guid? AuditedBy { get; private set; }
    public string? AuditNotes { get; private set; }
    public bool SlaExcluded { get; private set; }
    public bool IsOnHoldPayment { get; private set; }
    public Guid? SolutionTypeId { get; private set; }
    public decimal? PriorityScore { get; private set; }
    public decimal? ImpactScore { get; private set; }
    public decimal? UrgencyScore { get; private set; }
    public decimal? SentimentScore { get; private set; }
    public decimal? SlaSeverityScore { get; private set; }
    public decimal? TypeWeight { get; private set; }
    public decimal? TierWeight { get; private set; }
    public DateTimeOffset? PriorityScoreAt { get; private set; }
    public bool ForceP1 { get; private set; }
    public bool IsAutoResolved { get; private set; }
    public int CustomerReplyCount { get; private set; }

    public CsatSurvey? CsatSurvey { get; private set; }

    private readonly List<TicketAssignmentLog> _ticketAssignmentLogs = new();
    public IReadOnlyCollection<TicketAssignmentLog> TicketAssignmentLogs => _ticketAssignmentLogs;

    private readonly List<TicketAttachment> _ticketAttachments = new();
    public IReadOnlyCollection<TicketAttachment> TicketAttachments => _ticketAttachments;

    private readonly List<TicketComment> _ticketComments = new();
    public IReadOnlyCollection<TicketComment> TicketComments => _ticketComments;

    private readonly List<TicketCustomField> _ticketCustomFields = new();
    public IReadOnlyCollection<TicketCustomField> TicketCustomFields => _ticketCustomFields;

    private readonly List<TicketStatusHistory> _ticketStatusHistories = new();
    public IReadOnlyCollection<TicketStatusHistory> TicketStatusHistories => _ticketStatusHistories;

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    private Ticket() { }

    private Ticket(
        Guid companyId, 
        Guid moduleId, 
        string title, 
        string description, 
        Guid? createdByUserId, 
        TicketType type, 
        TicketPriority priority, 
        TicketSource source,
        Guid? assignedAgentId, 
        Guid? contactId)
    {
        CompanyId = companyId;
        ModuleId = moduleId;
        Title = title;
        Description = description;
        CreatedByUserId = createdByUserId;
        Type = type;
        Priority = priority;
        Source = source;
        AssignedAgentId = assignedAgentId;
        ContactId = contactId;
        
        CustomerReplyCount = 0;
        CustomerCallTaken = false;
        SlaExcluded = false;
        IsAutoResolved = false;
        ForceP1 = false;
        Status = TicketStatus.New;
    }

    public static Ticket Create(
        Guid companyId, 
        Guid moduleId, 
        string subject, 
        string description, 
        TicketType type,
        TicketSource source = TicketSource.Portal,
        Guid? createdByUserId = null,
        TicketPriority priority = TicketPriority.Medium,
        Guid? assignedAgentId = null,
        Guid? contactId = null)
    {
        if (companyId == Guid.Empty)
            throw new TicketDomainException("Company ID is required.");

        if (moduleId == Guid.Empty)
            throw new TicketDomainException("Module ID is required.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new TicketDomainException("Ticket subject cannot be empty.");

        if (subject.Length < 5 || subject.Length > 100)
            throw new TicketDomainException("Title must be between 5 and 100 characters.");

        if (string.IsNullOrWhiteSpace(description))
            throw new TicketDomainException("Ticket description cannot be empty.");

        if (description.Length > 5000)
            throw new TicketDomainException("Description cannot exceed 5000 characters.");

        var ticket = new Ticket(
            companyId, 
            moduleId, 
            subject, 
            description, 
            createdByUserId,
            type,
            priority,
            source,
            assignedAgentId,
            contactId
        );

        ticket._ticketStatusHistories.Add(TicketStatusHistory.Create(ticket.Id, null, TicketStatus.New, createdByUserId, "Ticket Created"));

        return ticket;
    }

    public void SetTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
            throw new TicketDomainException("Ticket number cannot be empty.");

        TicketNumber = ticketNumber;

        _domainEvents.Add(new TicketCreatedDomainEvent(
            Id,
            TicketNumber,
            Title,
            CreatedByUserId,
            AssignedAgentId,
            Type,
            Priority,
            CompanyId
        ));
    }

    public void UpdateTicket(string title, string description, TicketPriority priority, TicketType type, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 5 || title.Length > 100)
            throw new TicketDomainException("Title must be between 5 and 100 characters.");

        if (string.IsNullOrWhiteSpace(description) || description.Length > 5000)
            throw new TicketDomainException("Description must be between 1 and 5000 characters.");

        Title = title;
        Description = description;
        Priority = priority;
        Type = type;

        Touch(modifiedBy);
    }

    public void ChangeStatus(TicketStatus newStatus, Guid changedBy, string? reason = null)
    {
        if (Status == newStatus)
            return;
        
        var oldStatus = Status;
        Status = newStatus;

        _ticketStatusHistories.Add(TicketStatusHistory.Create(Id, oldStatus, newStatus, changedBy, reason));
        
        _domainEvents.Add(new TicketStatusChangedDomainEvent(Id, oldStatus, newStatus, changedBy));

        Touch(changedBy);
    }

    private void Touch(Guid userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    public void AssignAgent(Guid agentId, Guid assignedBy, string? notes = null)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Cannot assign agents to a closed ticket.");
        }

        if (agentId == Guid.Empty)
        {
            throw new TicketDomainException("Agent ID cannot be empty.");
        }

        if (AssignedAgentId == agentId)
        {
            return;
        }

        var previousAgent = AssignedAgentId;
        AssignedAgentId = agentId;

        _ticketAssignmentLogs.Add(TicketAssignmentLog.Create(Id, previousAgent, agentId, assignedBy, notes));

        if (Status == TicketStatus.New)
        {
            ChangeStatus(TicketStatus.Open, assignedBy, "Assigned Agent");
        }

        _domainEvents.Add(new TicketAssignedDomainEvent(Id, TicketNumber, agentId, assignedBy));

        Touch(assignedBy);
    }

    public void AssignGroup(Guid groupId, Guid modifiedBy)
    {
        if (groupId == Guid.Empty)
            throw new TicketDomainException("Group ID cannot be empty.");

        GroupId = groupId;
        Touch(modifiedBy);
    }

    public void AddComment(TicketComment comment, Guid addedBy)
    {
        ArgumentNullException.ThrowIfNull(comment);

        if (comment.IsCustomerReply)
        {
            CustomerReplyCount++;
        }
        else
        {
            Touch(addedBy);
        }

        _ticketComments.Add(comment);

        _domainEvents.Add(new TicketCommentAddedDomainEvent(
            Id,
            comment.Id,
            comment.Body,
            comment.MentionedUsers
        ));
    }

    public void Resolve(Guid resolvedBy, string resolutionSummary)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Cannot resolve a closed ticket.");
        }

        ChangeStatus(TicketStatus.Resolved, resolvedBy, resolutionSummary);
        _domainEvents.Add(new TicketResolvedDomainEvent(Id, TicketNumber, resolvedBy));
    }

    public void ProvideRootCauseAnalysis(string rca, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(rca))
            throw new TicketDomainException("RCA text cannot be empty.");

        Rca = rca;
        Touch(modifiedBy);
    }

    public void Reopen(Guid reopenedBy, string reason)
    {
        if (Status != TicketStatus.Resolved && Status != TicketStatus.Closed)
        {
            throw new TicketDomainException("Only resolved or closed tickets can be reopened.");
        }

        ChangeStatus(TicketStatus.Reopened, reopenedBy, reason);
        _domainEvents.Add(new TicketReopenedDomainEvent(Id, TicketNumber, reopenedBy));
    }

    public void Close(Guid closedBy, string notes)
    {
        if (Status != TicketStatus.Resolved)
            throw new TicketDomainException("Only resolved tickets can be closed.");

        ChangeStatus(TicketStatus.Closed, closedBy, notes);
        _domainEvents.Add(new TicketClosedDomainEvent(Id, TicketNumber, closedBy));
    }

    public void AddAttachment(TicketAttachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        _ticketAttachments.Add(attachment);
    }
}
