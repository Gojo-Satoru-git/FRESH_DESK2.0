using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// AI suggestions shown to agent on new ticket. Two rows per ticket: troubleshooting_steps and similar_tickets. agent_rating feeds the ML training pipeline. ai_rating also stored redundantly on tickets.ai_rating for quick dashboard reporting.
/// </summary>
public partial class AiSuggestionLog
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? AgentId { get; set; }

    public string SuggestionContent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User? Agent { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
