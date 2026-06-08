using System;
using System.Collections.Generic;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record UpdateTicketCommand(
    Guid TicketId,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketCategory Category,
    List<string> Tags,
    Guid ModifiedBy
) : IRequest<Guid>;
