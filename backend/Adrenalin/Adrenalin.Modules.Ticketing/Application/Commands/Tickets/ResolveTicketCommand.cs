using Adrenalin.SharedKernel.Mediator;
using System;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record ResolveTicketCommand(
    Guid TicketId,
    Guid ResolvedBy,
    string ResolutionSummary
) : IRequest<Guid>;
