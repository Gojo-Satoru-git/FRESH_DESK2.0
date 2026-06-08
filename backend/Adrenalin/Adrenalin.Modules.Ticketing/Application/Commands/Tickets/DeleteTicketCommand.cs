using System;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record DeleteTicketCommand(
    Guid TicketId,
    Guid PerformedBy
) : IRequest<Guid>;
