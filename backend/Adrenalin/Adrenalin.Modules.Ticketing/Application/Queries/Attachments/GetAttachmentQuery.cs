using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetAttachmentQuery(
    Guid TicketId,
    Guid AttachmentId
) : IRequest<AttachmentStreamResponse?>;
