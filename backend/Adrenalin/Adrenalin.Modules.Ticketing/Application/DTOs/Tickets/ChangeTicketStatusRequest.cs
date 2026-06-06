using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record ChangeTicketStatusRequest(
    TicketStatus NewStatus,
    Guid ChangedBy,
    string? Reason
);