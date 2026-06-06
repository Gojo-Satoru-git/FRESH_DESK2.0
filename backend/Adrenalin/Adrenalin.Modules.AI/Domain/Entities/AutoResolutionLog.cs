using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.AI.Domain.Entities;

public sealed class AutoResolutionLog : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid KbArticleId { get; private set; }
    public string MatchPhase { get; private set; }
    public decimal? SemanticSimilarity { get; private set; }
    public decimal FinalConfidence { get; private set; }
    public decimal ConfidenceThreshold { get; private set; }
    public string ActionTaken { get; private set; }
    public DateTimeOffset? ResolutionSentAt { get; private set; }
    public string? ResolutionChannel { get; private set; }
    public string? BlockedReason { get; private set; }
    public bool WasReopened { get; private set; }
    public DateTimeOffset? ReopenedAt { get; private set; }
    public string? ReopenReason { get; private set; }
    public DateTimeOffset MatchedAt { get; private set; }

    private readonly List<string> _keywordMatches = new();
    public IReadOnlyCollection<string> KeywordMatches => _keywordMatches.AsReadOnly();
    private AutoResolutionLog() 
    { 
        MatchPhase = string.Empty;
        ActionTaken = string.Empty;
    }

    public static AutoResolutionLog RecordAttempt(Guid ticketId, Guid kbArticleId, string matchPhase, decimal finalConfidence, decimal confidenceThreshold, IEnumerable<string>? keywordMatches = null, decimal? semanticSimilarity = null)
    {
        var log = new AutoResolutionLog
        {
            TicketId = ticketId,
            KbArticleId = kbArticleId,
            MatchPhase = matchPhase,
            FinalConfidence = finalConfidence,
            ConfidenceThreshold = confidenceThreshold,
            SemanticSimilarity = semanticSimilarity,
            MatchedAt = DateTimeOffset.UtcNow,
            ActionTaken = "Evaluating" // Default state
        };

        if (keywordMatches != null)
        {
            log._keywordMatches.AddRange(keywordMatches);
        }

        return log;
    }

    public void MarkAsSent(string channel)
    {
        ActionTaken = "ResolutionSent";
        ResolutionChannel = channel;
        ResolutionSentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsBlocked(string reason)
    {
        ActionTaken = "Blocked";
        BlockedReason = reason;
    }

    public void RecordReopen(string reason)
    {
        WasReopened = true;
        ReopenReason = reason;
        ReopenedAt = DateTimeOffset.UtcNow;
    }
}