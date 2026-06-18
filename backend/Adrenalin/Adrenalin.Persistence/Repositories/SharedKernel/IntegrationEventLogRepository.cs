using Microsoft.EntityFrameworkCore;
using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories;

public sealed class IntegrationEventLogRepository : IIntegrationEventLogRepository
{
    private readonly AdrenalinDbContext _db;

    public IntegrationEventLogRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasEventBeenProcessedAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _db.IntegrationEventLogs.AnyAsync(l => l.EventId == eventId, ct);
    }

    public async Task MarkEventAsProcessedAsync(IntegrationEventLog log, CancellationToken ct = default)
    {
        await _db.IntegrationEventLogs.AddAsync(log, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}
