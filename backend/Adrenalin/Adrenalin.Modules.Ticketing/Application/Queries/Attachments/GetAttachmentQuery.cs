using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Attachments;

public sealed record GetAttachmentQuery(
    Guid TicketId,
    Guid AttachmentId
) : IRequest<AttachmentStreamResponse?>;
