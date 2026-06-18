using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.SharedKernel.Interfaces;

public interface IIntegrationEventLogRepository
{
    Task<bool> HasEventBeenProcessedAsync(Guid eventId, CancellationToken ct = default);
    Task MarkEventAsProcessedAsync(IntegrationEventLog log, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
