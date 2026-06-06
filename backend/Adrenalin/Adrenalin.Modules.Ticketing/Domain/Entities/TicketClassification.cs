using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketClassification : BaseEntity
{
    public Guid TicketId { get; private set; }

    public string? ClassifierLabel { get; private set; }

    public decimal? ClassifierConfidence { get; private set; }

    public bool ClassifierAutoRouted { get; private set; }

    public string? RetrievalTopKLabels { get; private set; }

    public string? RetrievalConsensusLabel { get; private set; }

    public bool RetrievalDiscrepancy { get; private set; }

    public bool LlmInvoked { get; private set; }

    public string? LlmFinalLabel { get; private set; }

    public string? LlmReasoning { get; private set; }

    public string FinalLabel { get; private set; } = null!;

    public decimal? FinalConfidence { get; private set; }

    public string? ModelVersion { get; private set; }

    public DateTime ClassifiedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;
}
