using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class;
}
