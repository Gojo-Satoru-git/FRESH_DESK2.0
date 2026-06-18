using System;
using MediatR;

namespace Adrenalin.Modules.AI.Application.Commands;

public sealed record ImproveGrammarCommand(Guid TicketId, string Content) : IRequest<string>;
