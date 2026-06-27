using Adrenalin.SharedKernel.Mediator;
using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    string Priority,
    string Type,
    Guid? ActorId = null,
    bool IsCustomer = false,
    string? SenderEmail = null,
    Guid? AssigneeId = null,
    string? ModuleName = null
) : IRequest<Guid>;