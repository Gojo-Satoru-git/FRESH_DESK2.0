using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public record AssignTicketCommand(
    Guid TicketId,
    Guid TriggeredBy,
    bool IsAutoAssignment,
    Guid? OverrideAgentId = null,
    Guid? OverrideGroupId = null
) : IRequest<Result<AssignTicketResult>>;