using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;
public sealed class TicketAttachmentRepository : ITicketAttachmentRepository
{
    private readonly AdrenalinDbContext _context;

    public TicketAttachmentRepository(AdrenalinDbContext context)
    {
        _context = context;
    }

    public async Task<TicketAttachment?> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        return await _context.TicketAttachments
            .FirstOrDefaultAsync(
            x => x.Id == attachmentId, cancellationToken);
    }

    public async Task AddAsync(TicketAttachment attachment, CancellationToken cancellationToken)
    {
        await _context.TicketAttachments
            .AddAsync(attachment, cancellationToken);
    }

    public void Remove(TicketAttachment attachment)
    {
        _context.TicketAttachments.Remove(attachment);
    }
}