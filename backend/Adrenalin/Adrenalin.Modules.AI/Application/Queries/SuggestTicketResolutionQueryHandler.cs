using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Queries;

internal sealed class SuggestTicketResolutionQueryHandler : IRequestHandler<SuggestTicketResolutionQuery, ResolutionSuggestionDto>
{
    private readonly IAiProvider _aiProvider;

    public SuggestTicketResolutionQueryHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public async Task<ResolutionSuggestionDto> Handle(SuggestTicketResolutionQuery request, CancellationToken cancellationToken)
    {
        // Flow: Load Ticket, Load KB Articles
        var mockContent = $"Ticket Context for {request.TicketId}";

        // Call IAiProvider
        var suggestion = await _aiProvider.GenerateResolutionSuggestionAsync(mockContent, cancellationToken);
        var kbArticles = await _aiProvider.SuggestKnowledgeBaseArticlesAsync(mockContent, cancellationToken);

        return new ResolutionSuggestionDto(
            request.TicketId,
            "Hardware",
            "High",
            suggestion,
            kbArticles,
            0.88m);
    }
}
