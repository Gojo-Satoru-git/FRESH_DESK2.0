using System;
using MediatR;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.Modules.AI.Application.Queries;

public sealed record GenerateTicketSummaryQuery(Guid TicketId) : IRequest<TicketSummaryDto>;
