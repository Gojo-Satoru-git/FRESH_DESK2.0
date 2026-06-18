namespace Adrenalin.Modules.AI.Application.DTOs;

public sealed record TicketSummaryDto(
    Guid TicketId,
    string Summary,
    DateTime GeneratedAt,
    string ModelName,
    decimal ConfidenceScore);
