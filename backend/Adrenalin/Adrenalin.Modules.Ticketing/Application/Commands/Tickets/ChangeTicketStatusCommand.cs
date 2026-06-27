using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Tickets;

public sealed record ChangeTicketStatusCommand(Guid TicketId, TicketStatus NewStatus, Guid ChangedBy, string? Reason)
    : IRequest<Guid>;
