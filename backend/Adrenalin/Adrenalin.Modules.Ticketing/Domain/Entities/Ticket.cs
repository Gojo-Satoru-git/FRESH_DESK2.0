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
    
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public TicketStatus Status { get; private set; } = TicketStatus.New;
    public TicketType Type { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketCategory Category { get; private set; }
    public string? ModuleName { get; private set; }
    public string? Department { get; private set; }
    public string? Region { get; private set; }

    public string? ProductType { get; private set; }
    public string? SolutionType { get; private set; }
    public string? FixType { get; private set; }
    public string? Rca { get; private set; }
    public bool CustomerCallTaken { get; private set; }
    public string? LinkedJiraId { get; private set; }
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
    public DateTime? ModifiedAt { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    public CsatSurvey? CsatSurvey { get; private set; }
    public TicketClassification? TicketClassification { get; private set; }

    private readonly List<TicketAssignmentLog> _ticketAssignmentLogs = new();
    public IReadOnlyCollection<TicketAssignmentLog> TicketAssignmentLogs => _ticketAssignmentLogs.AsReadOnly();

    private readonly List<TicketAttachment> _ticketAttachments = new();
    public IReadOnlyCollection<TicketAttachment> TicketAttachments => _ticketAttachments.AsReadOnly();

    private readonly List<TicketComment> _ticketComments = new();
    public IReadOnlyCollection<TicketComment> TicketComments => _ticketComments.AsReadOnly();

    private readonly List<TicketCustomField> _ticketCustomFields = new();
    public IReadOnlyCollection<TicketCustomField> TicketCustomFields => _ticketCustomFields.AsReadOnly();

    private readonly List<TicketRiskScore> _ticketRiskScores = new();
    public IReadOnlyCollection<TicketRiskScore> TicketRiskScores => _ticketRiskScores.AsReadOnly();

    private readonly List<TicketStatusHistory> _ticketStatusHistories = new();
    public IReadOnlyCollection<TicketStatusHistory> TicketStatusHistories => _ticketStatusHistories.AsReadOnly();

    private readonly List<TicketWatcher> _ticketWatchers = new();
    public IReadOnlyCollection<TicketWatcher> TicketWatchers => _ticketWatchers.AsReadOnly();

    private readonly List<TicketRelation> _ticketRelations = new();
    public IReadOnlyCollection<TicketRelation> TicketRelations => _ticketRelations.AsReadOnly();

    private readonly List<TicketRelation> _childRelations = new();
    public IReadOnlyCollection<TicketRelation> ChildRelations => _childRelations.AsReadOnly();

    private readonly List<TicketTag> _ticketTags = new();
    public IReadOnlyCollection<TicketTag> TicketTags => _ticketTags.AsReadOnly();

    private readonly List<TicketActivity> _ticketActivities = new();
    public IReadOnlyCollection<TicketActivity> TicketActivities => _ticketActivities.AsReadOnly();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        { TicketStatus.New,        new[] { TicketStatus.Open } },
        { TicketStatus.Open,       new[] { TicketStatus.Assigned } },
        { TicketStatus.Assigned,   new[] { TicketStatus.InProgress } },
        { TicketStatus.InProgress, new[] { TicketStatus.Pending, TicketStatus.Resolved } },
        { TicketStatus.Pending,    new[] { TicketStatus.InProgress } },
        { TicketStatus.Resolved,   new[] { TicketStatus.Closed, TicketStatus.Reopened } },
        { TicketStatus.Closed,     new[] { TicketStatus.Reopened } },
        { TicketStatus.Reopened,   new[] { TicketStatus.Assigned } },
    };

    private Ticket() { }

    private Ticket(Guid companyId, Guid moduleId, string title, string description, Guid? createdByUserId, TicketCategory category, TicketPriority priority, string? moduleName, Guid? assignedAgentId, string? department, string? region, Guid? contactId)
    {
        CompanyId = companyId;
        ModuleId = moduleId;
        Title = title;
        Description = description;
        CreatedByUserId = createdByUserId;
        Category = category;
        Priority = priority;
        ModuleName = moduleName;
        AssignedAgentId = assignedAgentId;
        Department = department;
        Region = region;
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
        Guid? createdByUserId = null,
        TicketCategory category = TicketCategory.Support,
        TicketPriority priority = TicketPriority.Medium,
        string? moduleName = null,
        Guid? assignedAgentId = null,
        string? department = null,
        string? region = null,
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
            category,
            priority,
            moduleName,
            assignedAgentId,
            department,
            region,
            contactId
        );

        ticket.AddActivity("Ticket Created", null, TicketStatus.New.ToString(), createdByUserId);

        ticket._ticketStatusHistories.Add(TicketStatusHistory.Create(ticket.Id, null, TicketStatus.New, createdByUserId, "Ticket Created"));

        return ticket;
    }

    public void SetTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
            throw new TicketDomainException("Ticket number cannot be empty.");

        TicketNumber = ticketNumber;

        // Raise the created event only after the ticket number is properly assigned
        _domainEvents.Add(new TicketCreatedDomainEvent(
            Id,
            TicketNumber,
            Title,
            CreatedByUserId,
            AssignedAgentId,
            Category,
            Priority,
            Department,
            Region
        ));
    }

    public void UpdateTicket(string title, string description, TicketPriority priority, TicketCategory category, List<string> tags, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 5 || title.Length > 100)
            throw new TicketDomainException("Title must be between 5 and 100 characters.");

        if (string.IsNullOrWhiteSpace(description) || description.Length > 5000)
            throw new TicketDomainException("Description must be between 1 and 5000 characters.");

        var oldTitle = Title;
        var oldDesc = Description;
        var oldPriority = Priority;
        var oldCategory = Category;

        Title = title;
        Description = description;
        Priority = priority;
        Category = category;

        // Only log activities for values that actually changed
        if (oldTitle != Title)
            AddActivity("Title Changed", oldTitle, Title, modifiedBy);
        if (oldDesc != Description)
            AddActivity("Description Changed", oldDesc, Description, modifiedBy);
        if (oldPriority != Priority)
            AddActivity("Priority Changed", oldPriority.ToString(), Priority.ToString(), modifiedBy);
        if (oldCategory != Category)
            AddActivity("Category Changed", oldCategory.ToString(), Category.ToString(), modifiedBy);

        // Manage tags
        _ticketTags.Clear();
        foreach (var tag in tags)
        {
            _ticketTags.Add(TicketTag.Create(Id, tag));
        }

        Touch(modifiedBy);
    }

    public void ChangeStatus(TicketStatus newStatus, Guid changedBy, string? reason = null)
    {
        if (Status == newStatus)
            return;

        if (AllowedTransitions.TryGetValue(Status, out var allowed) && !allowed.Contains(newStatus))
            throw new TicketDomainException($"Invalid state transition from {Status} to {newStatus}.");
        
        var oldStatus = Status;
        Status = newStatus;

        AddActivity("Status Changed", oldStatus.ToString(), newStatus.ToString(), changedBy);

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

        AddActivity("Assignee Changed", previousAgent?.ToString(), agentId.ToString(), assignedBy);

        _ticketAssignmentLogs.Add(TicketAssignmentLog.Create(Id, previousAgent, agentId, assignedBy, notes));

        // Auto transition from Open to Assigned if assigned
        if (Status == TicketStatus.Open)
        {
            ChangeStatus(TicketStatus.Assigned, assignedBy, "Assigned Agent");
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

    public void UpdateScores(decimal priority, decimal impact, decimal urgency, Guid modifiedBy)
    {
        PriorityScore = priority;
        ImpactScore = impact;
        UrgencyScore = urgency;
        PriorityScoreAt = DateTimeOffset.UtcNow;
        Touch(modifiedBy);
    }

    public void LinkToJira(string jiraId, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(jiraId))
            throw new TicketDomainException("Jira ID cannot be empty when linking.");

        LinkedJiraId = jiraId;
        Touch(modifiedBy);
    }

    public void MarkCustomerCallTaken(Guid modifiedBy)
    {
        CustomerCallTaken = true;
        Touch(modifiedBy);
    }

    public void AddWatcher(Guid userId, Guid addedBy)
    {
        if (_ticketWatchers.Exists(w => w.UserId == userId))
            throw new TicketDomainException("User is already watching this ticket.");

        var watcher = TicketWatcher.Create(Id, userId, addedBy);
        _ticketWatchers.Add(watcher);
    }

    public void RemoveWatcher(Guid userId)
    {
        var watcher = _ticketWatchers.FirstOrDefault(x => x.UserId == userId);
        if (watcher is null)
        {
            throw new TicketDomainException("Watcher not found.");
        }
        _ticketWatchers.Remove(watcher);
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

        AddActivity("Comment Added", null, comment.Body, addedBy);

        _domainEvents.Add(new TicketCommentAddedDomainEvent(
            Id,
            comment.Id,
            comment.Body,
            comment.MentionedUsers
        ));
    }

    public void AddRelation(TicketRelation relation)
    {
        ArgumentNullException.ThrowIfNull(relation);

        if (relation.ParentTicketId != Id && relation.ChildTicketId != Id)
        {
            throw new TicketDomainException("Relation does not belong to this ticket.");
        }

        if (relation.ParentTicketId == relation.ChildTicketId)
        {
            throw new TicketDomainException("A ticket cannot relate to itself.");
        }

        if (_ticketRelations.Any(x =>
            x.ParentTicketId == relation.ParentTicketId &&
            x.ChildTicketId == relation.ChildTicketId &&
            x.RelationType == relation.RelationType))
        {
            throw new TicketDomainException("Relation already exists.");
        }

        _ticketRelations.Add(relation);
    }

    public void Resolve(Guid resolvedBy, string resolutionSummary)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Cannot resolve a closed ticket.");
        }

        ChangeStatus(TicketStatus.Resolved, resolvedBy, resolutionSummary);
        ResolvedAt = DateTimeOffset.UtcNow;
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
        ClosedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new TicketClosedDomainEvent(Id, TicketNumber, closedBy));
    }

    public void AddAttachment(TicketAttachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        _ticketAttachments.Add(attachment);
    }

    public void MoveComment(Guid commentId, Ticket destinationTicket)
    {
        ArgumentNullException.ThrowIfNull(destinationTicket);

        var comment = _ticketComments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
        {
            throw new TicketDomainException("Comment not found on this ticket.");
        }

        _ticketComments.Remove(comment);
        comment.MoveToTicket(destinationTicket.Id);
        destinationTicket._ticketComments.Add(comment);

        var commentAttachments = _ticketAttachments.Where(a => a.CommentId == commentId).ToList();
        foreach (var attachment in commentAttachments)
        {
            _ticketAttachments.Remove(attachment);
            attachment.MoveToTicket(destinationTicket.Id);
            destinationTicket._ticketAttachments.Add(attachment);
        }
    }

    public void MoveAttachment(Guid attachmentId, Ticket destinationTicket)
    {
        ArgumentNullException.ThrowIfNull(destinationTicket);

        var attachment = _ticketAttachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
        {
            throw new TicketDomainException("Attachment not found on this ticket.");
        }

        _ticketAttachments.Remove(attachment);
        attachment.MoveToTicket(destinationTicket.Id);
        destinationTicket._ticketAttachments.Add(attachment);
    }

    public void MergeInto(Guid mergedBy, string masterTicketNumber)
    {
        if (string.IsNullOrWhiteSpace(masterTicketNumber))
        {
            throw new TicketDomainException("Master ticket number is required.");
        }

        if(Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Ticket is already closed.");
        }

        var previousStatus = Status;
        Status = TicketStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;

        _ticketStatusHistories.Add(
            TicketStatusHistory.Create(
                Id,
                previousStatus,
                TicketStatus.Closed,
                mergedBy,
                $"Merged into {masterTicketNumber}"));

        AddActivity("Ticket Merged", previousStatus.ToString(), $"Closed (Merged into {masterTicketNumber})", mergedBy);

        _domainEvents.Add(new TicketMergedDomainEvent(Id, TicketNumber, masterTicketNumber, mergedBy));

        Touch(mergedBy);
    }

    public void AddTag(string tagName, Guid? addedBy)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return;
        if (!_ticketTags.Any(t => t.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
        {
            _ticketTags.Add(TicketTag.Create(Id, tagName));
            AddActivity("Tag Added", null, tagName, addedBy);
        }
    }

    public void AddActivity(string activityType, string? oldValue, string? newValue, Guid? performedBy)
    {
        _ticketActivities.Add(TicketActivity.Create(Id, activityType, oldValue, newValue, performedBy));
    }
}
