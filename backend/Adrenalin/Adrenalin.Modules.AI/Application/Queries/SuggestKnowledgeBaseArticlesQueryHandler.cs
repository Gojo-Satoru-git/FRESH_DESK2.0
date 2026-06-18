using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;

namespace Adrenalin.Modules.AI.Application.Queries;

internal sealed class SuggestKnowledgeBaseArticlesQueryHandler : IRequestHandler<SuggestKnowledgeBaseArticlesQuery, IReadOnlyList<string>>
{
    private readonly IAiProvider _aiProvider;

    public SuggestKnowledgeBaseArticlesQueryHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public Task<IReadOnlyList<string>> Handle(SuggestKnowledgeBaseArticlesQuery request, CancellationToken cancellationToken)
    {
        var mockContent = $"Ticket Content {request.TicketId}";
        return _aiProvider.SuggestKnowledgeBaseArticlesAsync(mockContent, cancellationToken);
    }
}
