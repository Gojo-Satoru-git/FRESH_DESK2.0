using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Queries;

internal sealed class GenerateTicketSummaryQueryHandler : IRequestHandler<GenerateTicketSummaryQuery, TicketSummaryDto>
{
    private readonly IAiProvider _aiProvider;

    public GenerateTicketSummaryQueryHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public async Task<TicketSummaryDto> Handle(GenerateTicketSummaryQuery request, CancellationToken cancellationToken)
    {
        // Flow: Load Ticket, Load Comments, Load Timeline, Build Ticket Context
        var mockContent = $"Ticket Context for {request.TicketId}";

        // Call IAiProvider
        var summary = await _aiProvider.GenerateSummaryAsync(mockContent, cancellationToken);

        return new TicketSummaryDto(
            request.TicketId,
            summary,
            DateTime.UtcNow,
            "MockModel-v1",
            0.95m);
    }
}
