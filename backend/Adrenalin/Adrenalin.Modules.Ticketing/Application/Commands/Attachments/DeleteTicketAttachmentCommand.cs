using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Attachments;

public sealed record DeleteTicketAttachmentCommand(
    Guid TicketId,
    Guid AttachmentId
) : IRequest<Guid>;
