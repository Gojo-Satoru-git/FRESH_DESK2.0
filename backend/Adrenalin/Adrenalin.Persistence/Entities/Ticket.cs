using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Central transactional entity. graph_id is resolved once at creation via scope engine. version_id/module_id/sub_module_id are normalized FK refs per Addendum v7. sla_excluded=true during hypercare/delivery mode. is_on_hold_payment for payment holds.
/// </summary>
public partial class Ticket
{
    public Guid Id { get; set; }

    public string? TicketNumber { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? ContactId { get; set; }

    public Guid? GroupId { get; set; }

    public Guid? AssignedAgentId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? GraphId { get; set; }

    public Guid? VersionId { get; set; }

    public Guid ModuleId { get; set; }

    public Guid? SubModuleId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ProductType { get; set; }

    public string? SolutionType { get; set; }

    public string? FixType { get; set; }

    public string? Rca { get; set; }

    public bool CustomerCallTaken { get; set; }

    public string? LinkedJiraId { get; set; }

    public Guid? AuditedBy { get; set; }

    public string? AuditNotes { get; set; }

    public bool SlaExcluded { get; set; }

    public bool IsOnHoldPayment { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    /// <summary>
    /// FK to lookup.solution_types. Replaces the free-text solution_type varchar column. Both columns kept during migration; remove solution_type varchar after data backfill.
    /// </summary>
    public Guid? SolutionTypeId { get; set; }

    /// <summary>
    /// Computed weighted priority score (0–5). Formula: (0.30×impact) + (0.20×urgency) + (0.15×sentiment) + (0.15×sla_severity) + (0.10×type) + (0.10×tier). Mapped: ≥4.5=P1(Urgent), 3.5–4.49=P2(High), 2.5–3.49=P3(Medium), &lt;2.5=P4(Low).
    /// </summary>
    public decimal? PriorityScore { get; set; }

    public decimal? ImpactScore { get; set; }

    public decimal? UrgencyScore { get; set; }

    public decimal? SentimentScore { get; set; }

    public decimal? SlaSeverityScore { get; set; }

    public decimal? TypeWeight { get; set; }

    public decimal? TierWeight { get; set; }

    /// <summary>
    /// Timestamp of last priority score computation. Re-evaluation loop updates this on every customer reply or SLA status change.
    /// </summary>
    public DateTime? PriorityScoreAt { get; set; }

    /// <summary>
    /// TRUE when a force-P1 rule fires: system_down, security_breach, or sla_breach_imminent. Overrides the computed priority_score regardless of value.
    /// </summary>
    public bool ForceP1 { get; set; }

    /// <summary>
    /// TRUE when the auto-resolution engine closed this ticket without human intervention. Used for KPI: target is 30–40% of total tickets. Set by auto-resolution engine after confidence &gt; 0.85 match and solution sent.
    /// </summary>
    public bool IsAutoResolved { get; set; }

    /// <summary>
    /// Incremented by automation rule on every inbound customer comment. Re-evaluation loop: every increment triggers risk agent re-score. Multiple follow-ups (≥3) increase urgency_score by +1 in the scoring formula.
    /// </summary>
    public int CustomerReplyCount { get; set; }

    public virtual ICollection<AgentBadge> AgentBadges { get; set; } = new List<AgentBadge>();

    public virtual ICollection<AgentPoint> AgentPoints { get; set; } = new List<AgentPoint>();

    public virtual ICollection<AiSuggestionLog> AiSuggestionLogs { get; set; } = new List<AiSuggestionLog>();

    public virtual User? AssignedAgent { get; set; }

    public virtual User? AuditedByNavigation { get; set; }

    public virtual ICollection<AutoResolutionLog> AutoResolutionLogs { get; set; } = new List<AutoResolutionLog>();

    public virtual ICollection<AutomationExecutionLog> AutomationExecutionLogs { get; set; } = new List<AutomationExecutionLog>();

    public virtual Company Company { get; set; } = null!;

    public virtual Contact? Contact { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual CsatSurvey? CsatSurvey { get; set; }

    public virtual TicketStatusGraph? Graph { get; set; }

    public virtual Group? Group { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();

    public virtual SlaTicket? SlaTicket { get; set; }

    public virtual SolutionType? SolutionTypeNavigation { get; set; }

    public virtual SubModule? SubModule { get; set; }

    public virtual ICollection<TicketAssignmentLog> TicketAssignmentLogs { get; set; } = new List<TicketAssignmentLog>();

    public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();

    public virtual TicketClassification? TicketClassification { get; set; }

    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    public virtual ICollection<TicketCustomField> TicketCustomFields { get; set; } = new List<TicketCustomField>();

    public virtual ICollection<TicketRiskScore> TicketRiskScores { get; set; } = new List<TicketRiskScore>();

    public virtual ICollection<TicketStatusHistory> TicketStatusHistories { get; set; } = new List<TicketStatusHistory>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ProductVersion? Version { get; set; }
}
