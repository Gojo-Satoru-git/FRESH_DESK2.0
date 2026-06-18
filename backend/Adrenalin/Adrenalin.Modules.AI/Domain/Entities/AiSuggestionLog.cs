using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.AI.Domain.Entities;

public sealed class AiSuggestionLog : BaseEntity
{
    public Guid TicketId { get; private set; }
    public string RequestType { get; private set; } = string.Empty;
    public string PromptHash { get; private set; } = string.Empty;
    public string ResponseHash { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int TokensUsed { get; private set; }
    public long ExecutionTimeMs { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AiSuggestionLog() { }

    private AiSuggestionLog(
        Guid ticketId, 
        string requestType, 
        string promptHash, 
        string responseHash, 
        string provider, 
        string model, 
        int tokensUsed, 
        long executionTimeMs)
    {
        TicketId = ticketId;
        RequestType = requestType;
        PromptHash = promptHash;
        ResponseHash = responseHash;
        Provider = provider;
        Model = model;
        TokensUsed = tokensUsed;
        ExecutionTimeMs = executionTimeMs;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static AiSuggestionLog Create(
        Guid ticketId, 
        string requestType, 
        string promptHash, 
        string responseHash, 
        string provider, 
        string model, 
        int tokensUsed, 
        long executionTimeMs)
    {
        return new AiSuggestionLog(ticketId, requestType, promptHash, responseHash, provider, model, tokensUsed, executionTimeMs);
    }
}
