using Adrenalin.SharedKernel.Mediator;
using System;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record CloseTicketCommand(
    Guid TicketId,
    Guid ClosedBy,
    string Notes
) : IRequest<Guid>;
