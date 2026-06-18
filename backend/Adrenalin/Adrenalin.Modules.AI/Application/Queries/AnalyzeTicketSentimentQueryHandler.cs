using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Queries;

internal sealed class AnalyzeTicketSentimentQueryHandler : IRequestHandler<AnalyzeTicketSentimentQuery, SentimentAnalysisDto>
{
    private readonly IAiProvider _aiProvider;

    public AnalyzeTicketSentimentQueryHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public async Task<SentimentAnalysisDto> Handle(AnalyzeTicketSentimentQuery request, CancellationToken cancellationToken)
    {
        // Flow: Load Ticket, Load Comments
        var mockContent = $"Ticket Content {request.TicketId}";

        // Call IAiProvider
        var result = await _aiProvider.AnalyzeSentimentAsync(mockContent, cancellationToken);

        return new SentimentAnalysisDto(
            request.TicketId,
            result.Sentiment,
            result.Score,
            result.Reasoning);
    }
}
