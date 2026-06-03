using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Full audit of auto-resolution engine activity. One row per resolution attempt (successful or blocked). was_reopened=TRUE is the primary negative signal for the learning loop — the learning job reads this to raise confidence_threshold on kb_articles with high reopen rates. blocked_guardrail action means payroll/financial/legal guardrail fired.
/// </summary>
public partial class AutoResolutionLog
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid KbArticleId { get; set; }

    public string MatchPhase { get; set; } = null!;

    public List<string>? KeywordMatches { get; set; }

    public decimal? SemanticSimilarity { get; set; }

    public decimal FinalConfidence { get; set; }

    public decimal ConfidenceThreshold { get; set; }

    public string ActionTaken { get; set; } = null!;

    public DateTime? ResolutionSentAt { get; set; }

    public string? ResolutionChannel { get; set; }

    public string? BlockedReason { get; set; }

    public bool WasReopened { get; set; }

    public DateTime? ReopenedAt { get; set; }

    public string? ReopenReason { get; set; }

    public DateTime MatchedAt { get; set; }

    public virtual KbArticle KbArticle { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
