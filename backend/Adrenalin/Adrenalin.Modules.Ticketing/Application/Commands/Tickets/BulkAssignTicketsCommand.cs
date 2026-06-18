using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record BulkAssignTicketsCommand(
    List<Guid> TicketIds,
    Guid? OverrideAgentId,
    Guid? OverrideGroupId,
    Guid TriggeredBy
) : IRequest<Result<BulkAssignTicketsResult>>;

public sealed record BulkAssignTicketsResult(
    int SuccessfulCount,
    int FailedCount,
    List<string> Errors
);
