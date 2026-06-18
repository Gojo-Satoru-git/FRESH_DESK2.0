using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories;

public sealed class EmailMessageRepository : IEmailMessageRepository
{
    private readonly AdrenalinDbContext _db;

    public EmailMessageRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.EmailMessages.Include(e => e.Attachments).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailMessage?> GetByInternetMessageIdAsync(string internetMessageId, CancellationToken cancellationToken = default)
    {
        return await _db.EmailMessages.FirstOrDefaultAsync(e => e.InternetMessageId == internetMessageId, cancellationToken);
    }

    public async Task<EmailMessage?> GetByMessageIdAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return await _db.EmailMessages.FirstOrDefaultAsync(e => e.MessageId == messageId, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailMessage>> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default)
    {
        return await _db.EmailMessages
            .Where(x => x.ThreadId == threadId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        await _db.EmailMessages.AddAsync(emailMessage, cancellationToken);
    }

    public void Update(EmailMessage emailMessage)
    {
        _db.EmailMessages.Update(emailMessage);
    }
}
