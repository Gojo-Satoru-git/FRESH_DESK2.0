using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketAttachmentRepository
{
    Task<TicketAttachment?> GetByIdAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TicketAttachment attachment,
        CancellationToken cancellationToken = default);

    void Remove(
        TicketAttachment attachment);
}