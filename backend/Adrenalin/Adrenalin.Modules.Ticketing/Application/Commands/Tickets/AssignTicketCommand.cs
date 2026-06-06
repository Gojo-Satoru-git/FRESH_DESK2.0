using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record AssignTicketCommand(Guid TicketId, Guid AgentId, Guid AssignedBy, string? Notes)
    : IRequest<Guid>;
