namespace Adrenalin.Modules.AI.Application.DTOs;

public sealed record ResolutionSuggestionDto(
    Guid TicketId,
    string SuggestedCategory,
    string SuggestedPriority,
    string SuggestedReply,
    IReadOnlyList<string> SuggestedKbArticles,
    decimal ConfidenceScore);
