using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record ClaimTicketCommand(
    Guid TicketId,
    Guid AgentId
) : IRequest<Result<Guid>>;
