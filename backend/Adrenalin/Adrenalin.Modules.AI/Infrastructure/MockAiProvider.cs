using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.AI.Application.Contracts;

namespace Adrenalin.Modules.AI.Infrastructure;

public sealed class MockAiProvider : IAiProvider
{
    public Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult("Mock AI Summary");
    }

    public Task<string> GenerateResolutionSuggestionAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult("Please restart the application and retry.");
    }

    public Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SentimentAnalysisResult("Neutral", 0.5m, "No strong emotions detected in text."));
    }

    public Task<AutoCategorizationResult> CategorizeAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AutoCategorizationResult("General", "Inquiry", 0.90m));
    }

    public Task<IReadOnlyList<string>> SuggestSimilarHistoricalTicketsAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<string>>(new List<string> { "TCK-1001", "TCK-1042", "TCK-1108" });
    }

    public Task<IReadOnlyList<string>> SuggestKnowledgeBaseArticlesAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<string>>(new List<string> { "KB-001: Getting Started", "KB-045: Troubleshooting Login" });
    }

    public Task<string> ImproveGrammarAsync(string content, CancellationToken cancellationToken)
    {
        return Task.FromResult("Improved text: " + content);
    }
}
