using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record UploadTicketAttachmentCommand(
    Guid TicketId,
    Guid? CommentId,
    Stream Stream,
    string FileName,
    long Length,
    string ContentType,
    Guid UploadedBy
) : IRequest<Guid>;