namespace Adrenalin.Modules.AI.Application.DTOs;

public sealed record SentimentAnalysisDto(
    Guid TicketId,
    string Sentiment,
    decimal Score,
    string Reasoning);
