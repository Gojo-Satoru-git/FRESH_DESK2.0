using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// One row per ticket — full C-R-L (Classifier-Retrieval-LLM) pipeline audit. classifier_auto_routed=TRUE means phase 2+3 were skipped (confidence ≥ threshold). retrieval_discrepancy=TRUE triggered Phase 3 LLM. llm_invoked tracks LLM cost exposure. model_version enables A/B testing.
/// </summary>
public partial class TicketClassification
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string? ClassifierLabel { get; set; }

    public decimal? ClassifierConfidence { get; set; }

    public bool ClassifierAutoRouted { get; set; }

    public string? RetrievalTopKLabels { get; set; }

    public string? RetrievalConsensusLabel { get; set; }

    public bool RetrievalDiscrepancy { get; set; }

    public bool LlmInvoked { get; set; }

    public string? LlmFinalLabel { get; set; }

    public string? LlmReasoning { get; set; }

    public string FinalLabel { get; set; } = null!;

    public decimal? FinalConfidence { get; set; }

    public string? ModelVersion { get; set; }

    public DateTime ClassifiedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
