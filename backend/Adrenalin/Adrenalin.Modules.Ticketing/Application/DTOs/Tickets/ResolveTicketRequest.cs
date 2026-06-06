using System;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record ResolveTicketRequest(
    Guid ResolvedBy,
    string ResolutionSummary
);
