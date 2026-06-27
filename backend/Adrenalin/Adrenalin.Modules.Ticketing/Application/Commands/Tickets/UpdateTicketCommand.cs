using System;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record UpdateTicketCommand(
    Guid TicketId,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketType Type,
    Guid ModifiedBy
) : IRequest<Guid>;
