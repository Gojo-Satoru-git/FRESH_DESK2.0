using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface IEmailMessageRepository
{
    Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailMessage?> GetByInternetMessageIdAsync(string internetMessageId, CancellationToken cancellationToken = default);
    Task<EmailMessage?> GetByMessageIdAsync(string messageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailMessage>> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default);
    Task AddAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
    void Update(EmailMessage emailMessage);
}

public interface IProcessedEmailLogRepository
{
    Task<bool> HasBeenProcessedAsync(string internetMessageId, CancellationToken cancellationToken = default);
    Task AddLogAsync(ProcessedEmailLog log, CancellationToken cancellationToken = default);
}

public interface IEmailAliasRoutingRepository
{
    Task<EmailAliasRouting?> GetBestMatchAsync(string emailAddress, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailAliasRouting>> GetAllAsync(CancellationToken cancellationToken = default);
}
