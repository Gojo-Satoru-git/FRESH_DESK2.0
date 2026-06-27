using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public sealed record ChangeTicketStatusRequest(
    TicketStatus NewStatus,
    Guid ChangedBy,
    string? Reason
);