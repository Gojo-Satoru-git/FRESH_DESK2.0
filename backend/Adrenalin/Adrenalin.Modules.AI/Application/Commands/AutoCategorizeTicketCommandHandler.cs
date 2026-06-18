using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Adrenalin.Modules.AI.Application.Contracts;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Commands;

internal sealed class AutoCategorizeTicketCommandHandler : IRequestHandler<AutoCategorizeTicketCommand, TicketCategorizationDto>
{
    private readonly IAiProvider _aiProvider;

    public AutoCategorizeTicketCommandHandler(IAiProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public async Task<TicketCategorizationDto> Handle(AutoCategorizeTicketCommand request, CancellationToken cancellationToken)
    {
        // Flow: Load Ticket
        var mockContent = $"Ticket Content {request.TicketId}";

        // Call IAiProvider
        var result = await _aiProvider.CategorizeAsync(mockContent, cancellationToken);

        // Store Result, Update Suggested Category, Persist Audit Log
        // (Stubbed for now)

        return new TicketCategorizationDto(
            request.TicketId,
            result.Category,
            result.SubCategory,
            result.ConfidenceScore);
    }
}
