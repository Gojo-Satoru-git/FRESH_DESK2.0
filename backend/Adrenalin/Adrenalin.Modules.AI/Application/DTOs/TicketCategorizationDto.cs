using System;

namespace Adrenalin.Modules.AI.Application.DTOs;

public sealed record TicketCategorizationDto(
    Guid TicketId,
    string Category,
    string SubCategory,
    decimal ConfidenceScore);
