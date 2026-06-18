using System;
using MediatR;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Commands;

public sealed record AutoCategorizeTicketCommand(Guid TicketId) : IRequest<TicketCategorizationDto>;
