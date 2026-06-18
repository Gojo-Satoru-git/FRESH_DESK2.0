namespace Adrenalin.Modules.AI.Application.Contracts;

public sealed record SentimentAnalysisResult(string Sentiment, decimal Score, string Reasoning);

public sealed record AutoCategorizationResult(string Category, string SubCategory, decimal ConfidenceScore);

public interface IAiProvider
{
    Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken);
    
    Task<string> GenerateResolutionSuggestionAsync(string content, CancellationToken cancellationToken);
    
    Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content, CancellationToken cancellationToken);
    
    Task<AutoCategorizationResult> CategorizeAsync(string content, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> SuggestSimilarHistoricalTicketsAsync(string content, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<string>> SuggestKnowledgeBaseArticlesAsync(string content, CancellationToken cancellationToken);
    
    Task<string> ImproveGrammarAsync(string content, CancellationToken cancellationToken);
}
