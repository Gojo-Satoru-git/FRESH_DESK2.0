using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Full audit of every priority score computation run. Multiple rows per ticket because the re-evaluation loop fires on every customer reply and SLA status change. trigger_event shows what caused the re-score. customer_reply_count_at enables auditing how many replies pushed priority up.
/// </summary>
public partial class TicketRiskScore
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public decimal ImpactScore { get; set; }

    public decimal UsersAffectedScore { get; set; }

    public decimal ModulesAffectedScore { get; set; }

    public decimal UrgencyScore { get; set; }

    public List<string>? UrgencyKeywords { get; set; }

    public decimal SentimentScore { get; set; }

    public string? SentimentLabel { get; set; }

    public decimal SlaSeverityScore { get; set; }

    public decimal TypeWeight { get; set; }

    public decimal TierWeight { get; set; }

    public decimal FinalScore { get; set; }

    public string AssignedPriority { get; set; } = null!;

    public bool ForceP1Triggered { get; set; }

    public string? ForceP1Reason { get; set; }

    public bool BusinessCriticalBump { get; set; }

    public bool PilotProjectFlag { get; set; }

    public bool CompetitorThreatFlag { get; set; }

    public string? TriggerEvent { get; set; }

    public int? CustomerReplyCountAt { get; set; }

    public DateTime ComputedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
