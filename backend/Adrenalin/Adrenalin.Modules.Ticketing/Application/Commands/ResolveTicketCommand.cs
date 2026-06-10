using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public record ResolveTicketCommand(
    Guid TicketId,
    Guid AgentId,
    string ResolutionNote
) : IRequest<Result<bool>>;