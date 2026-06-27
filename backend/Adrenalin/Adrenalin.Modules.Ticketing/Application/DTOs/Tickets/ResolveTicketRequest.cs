using System;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record ResolveTicketRequest(
    Guid ResolvedBy,
    string ResolutionSummary
);
