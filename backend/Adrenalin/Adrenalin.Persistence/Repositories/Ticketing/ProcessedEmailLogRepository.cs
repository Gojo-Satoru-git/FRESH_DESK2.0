using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories;

public sealed class ProcessedEmailLogRepository : IProcessedEmailLogRepository
{
    private readonly AdrenalinDbContext _db;

    public ProcessedEmailLogRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasBeenProcessedAsync(string internetMessageId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<ProcessedEmailLog>().AnyAsync(l => l.InternetMessageId == internetMessageId, cancellationToken);
    }

    public async Task AddLogAsync(ProcessedEmailLog log, CancellationToken cancellationToken = default)
    {
        await _db.Set<ProcessedEmailLog>().AddAsync(log, cancellationToken);
    }
}
