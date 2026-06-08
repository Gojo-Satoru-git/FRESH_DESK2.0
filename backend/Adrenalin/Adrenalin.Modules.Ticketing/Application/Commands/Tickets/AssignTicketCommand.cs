using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.Modules.Ticketing.Application.DTOs;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public record AssignTicketCommand(
    Guid TicketId,
    Guid TriggeredBy,
    bool IsAutoAssignment,
    Guid? OverrideAgentId = null,
    Guid? OverrideGroupId = null
) : IRequest<Result<AssignTicketResult>>;