using System;
using System.Collections.Generic;
using MediatR;

namespace Adrenalin.Modules.AI.Application.Queries;

public sealed record SuggestSimilarHistoricalTicketsQuery(Guid TicketId) : IRequest<IReadOnlyList<string>>;
