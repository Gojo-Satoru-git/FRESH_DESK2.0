using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.AI.Domain.Entities;

public sealed class AiSuggestionLog : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid? AgentId { get; private set; }
    public string SuggestionContent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AiSuggestionLog()
    {
        SuggestionContent = string.Empty;
    }

    private AiSuggestionLog(Guid ticketId, Guid? agentId, string suggestionContext)
    {
        TicketId = ticketId;
        AgentId = agentId;
        SuggestionContent = suggestionContext;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static AiSuggestionLog Create(Guid ticketId, Guid? agentId, string suggestionContext)
    {
        if (string.IsNullOrWhiteSpace(suggestionContext))
        {
            throw new ArgumentException("Suggestion context is required.", nameof(suggestionContext));
        }

        return new AiSuggestionLog(ticketId, agentId, suggestionContext);
    }
}
