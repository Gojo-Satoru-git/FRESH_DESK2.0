using Adrenalin.SharedKernel.Mediator;
using System;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record CloseTicketCommand(
    Guid TicketId,
    Guid ClosedBy,
    string Notes
) : IRequest<Guid>;
