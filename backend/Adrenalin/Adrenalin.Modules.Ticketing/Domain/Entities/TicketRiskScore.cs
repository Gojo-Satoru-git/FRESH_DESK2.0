using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketRiskScore : BaseEntity
{
    public Guid TicketId { get; private set; }

    public decimal ImpactScore { get; private set; }

    public decimal UsersAffectedScore { get; private set; }

    public decimal ModulesAffectedScore { get; private set; }

    public decimal UrgencyScore { get; private set; }

    public List<string>? UrgencyKeywords { get; private set; }

    public decimal SentimentScore { get; private set; }

    public string? SentimentLabel { get; private set; }

    public decimal SlaSeverityScore { get; private set; }

    public decimal TypeWeight { get; private set; }

    public decimal TierWeight { get; private set; }

    public decimal FinalScore { get; private set; }

    public string AssignedPriority { get; private set; } = null!;

    public bool ForceP1Triggered { get; private set; }

    public string? ForceP1Reason { get; private set; }

    public bool BusinessCriticalBump { get; private set; }

    public bool PilotProjectFlag { get; private set; }

    public bool CompetitorThreatFlag { get; private set; }

    public string? TriggerEvent { get; private set; }

    public int? CustomerReplyCountAt { get; private set; }

    public DateTime ComputedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    private TicketRiskScore() { }

    public static TicketRiskScore Create(Guid ticketId, string assignedPriority, decimal finalScore)
    {
        return new TicketRiskScore
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AssignedPriority = assignedPriority,
            FinalScore = finalScore,
            ComputedAt = DateTime.UtcNow
        };
    }
}
