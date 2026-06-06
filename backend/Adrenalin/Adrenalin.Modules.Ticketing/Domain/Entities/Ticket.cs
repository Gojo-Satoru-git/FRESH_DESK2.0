using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Entities;
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
    public string Subject { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public TicketStatus Status { get; private set; } = TicketStatus.New;
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

    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        {TicketStatus.New, new[] { TicketStatus.Open } },
        {TicketStatus.Open, new[] { TicketStatus.InProgress, TicketStatus.PendingCustomer, TicketStatus.PendingInternal, TicketStatus.OnHold, TicketStatus.ProductRoadmap, TicketStatus.Resolved}},
        {TicketStatus.InProgress, new[] { TicketStatus.PendingCustomer, TicketStatus.PendingInternal, TicketStatus.OnHold, TicketStatus.Resolved }},
        {TicketStatus.PendingCustomer, new[] { TicketStatus.InProgress, TicketStatus.Resolved }},
        {TicketStatus.PendingInternal, new[] { TicketStatus.InProgress, TicketStatus.Resolved }},
        {TicketStatus.OnHold, new[] { TicketStatus.Open, TicketStatus.InProgress }},
        {TicketStatus.ProductRoadmap, new[] { TicketStatus.Open }},
        {TicketStatus.Resolved, new[] { TicketStatus.Closed, TicketStatus.Reopened }},
        {TicketStatus.Reopened, new[] { TicketStatus.InProgress, TicketStatus.PendingCustomer }},
        {TicketStatus.Closed, Array.Empty<TicketStatus>() }
    };

    private Ticket() { }

    private Ticket(Guid companyId, Guid moduleId, string subject, string description, Guid? createdByUserId)
    {
        CompanyId = companyId;
        ModuleId = moduleId;
        Subject = subject;
        Description = description;
        CreatedByUserId = createdByUserId;
        CustomerReplyCount = 0;
        CustomerCallTaken = false;
        SlaExcluded = false;
        IsAutoResolved = false;
        ForceP1 = false;
    }
    public static Ticket Create(Guid companyId, Guid moduleId, string subject, string description, Guid? createdByUserId = null)
    {
        if (companyId == Guid.Empty)
            throw new TicketDomainException("Company ID is required.");

        if (moduleId == Guid.Empty)
            throw new TicketDomainException("Module ID is required.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new TicketDomainException("Ticket subject cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new TicketDomainException("Ticket description cannot be empty.");

        var ticket = new Ticket(companyId, moduleId, subject, description, createdByUserId);

        ticket._ticketStatusHistories.Add(TicketStatusHistory.Create(ticket.Id, null, TicketStatus.New, createdByUserId, "Ticket Created"));

        return ticket;
    }
    public void ChangeStatus(TicketStatus newStatus, Guid changedBy, string? reason = null)
    {
        if (Status == newStatus)
            return;

        if (AllowedTransitions.TryGetValue(Status, out var allowed) && !allowed.Contains(newStatus))
            throw new TicketDomainException($"Invalid state transition from {Status} to {newStatus}.");
        
        var oldStatus = Status;
        Status = newStatus;

        _ticketStatusHistories.Add(TicketStatusHistory.Create(Id, oldStatus, newStatus, changedBy, reason));
        
        Touch(changedBy);
    }

    private void Touch(Guid? userId)
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

        _ticketAssignmentLogs.Add(TicketAssignmentLog.Create(Id,previousAgent,agentId,assignedBy,notes));

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
            Touch(null);
        }
        else
        {
            Touch(addedBy);
        }

        _ticketComments.Add(comment);
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

        if (!CustomerCallTaken)
        {
            throw new TicketDomainException("Customer call must be recorded before resolution.");
        }

        if (string.IsNullOrWhiteSpace(Rca))
        {
            throw new TicketDomainException("RCA is required before resolving.");
        }

        ChangeStatus(TicketStatus.Resolved, resolvedBy, resolutionSummary);
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
    }

    public void Close(Guid closedBy, string notes)
    {
        if (Status != TicketStatus.Resolved)
            throw new TicketDomainException("Only resolved tickets can be closed.");

        ChangeStatus(TicketStatus.Closed, closedBy, notes);
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

        _ticketStatusHistories.Add(
            TicketStatusHistory.Create(
                Id,
                previousStatus,
                TicketStatus.Closed,
                mergedBy,
                $"Merged into {masterTicketNumber}"));

        Touch(mergedBy);
    }
}
