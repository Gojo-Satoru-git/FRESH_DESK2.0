using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record LinkTicketRequest
(
    Guid ChildTicketId,
    TicketRelationType RelationType
);