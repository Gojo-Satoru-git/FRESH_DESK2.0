using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    string Priority,
    string Category,
    List<string>? Tags = null,
    Guid? ActorId = null,
    bool IsCustomer = false,
    string? SenderEmail = null,
    Guid? AssigneeId = null,
    string? ModuleName = null
) : IRequest<Guid>;