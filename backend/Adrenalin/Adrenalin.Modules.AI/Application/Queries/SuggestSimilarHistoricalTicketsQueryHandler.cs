using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;

namespace Adrenalin.Modules.AI.Application.Queries;

internal sealed class SuggestSimilarHistoricalTicketsQueryHandler : IRequestHandler<SuggestSimilarHistoricalTicketsQuery, IReadOnlyList<string>>
{
    private readonly IAiProvider _aiProvider;

    public SuggestSimilarHistoricalTicketsQueryHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public Task<IReadOnlyList<string>> Handle(SuggestSimilarHistoricalTicketsQuery request, CancellationToken cancellationToken)
    {
        var mockContent = $"Ticket Content {request.TicketId}";
        return _aiProvider.SuggestSimilarHistoricalTicketsAsync(mockContent, cancellationToken);
    }
}
