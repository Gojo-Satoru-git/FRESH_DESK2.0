using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Persistence.Outbox;

public sealed class OutboxEventBus : IEventBus
{
    private readonly AdrenalinDbContext _dbContext;

    public OutboxEventBus(AdrenalinDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = integrationEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredOn = DateTimeOffset.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);

        return Task.CompletedTask;
    }
}
