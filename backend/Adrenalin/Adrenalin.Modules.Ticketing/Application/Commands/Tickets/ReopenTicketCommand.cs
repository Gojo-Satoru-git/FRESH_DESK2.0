using Adrenalin.SharedKernel.Mediator;
using System;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record ReopenTicketCommand(
    Guid TicketId,
    Guid ReopenedBy,
    string Reason
) : IRequest<Guid>;
