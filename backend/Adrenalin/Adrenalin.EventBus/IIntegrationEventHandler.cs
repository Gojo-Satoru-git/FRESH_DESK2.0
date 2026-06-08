using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.EventBus;

public interface IIntegrationEventHandler<in T> where T : class
{
    Task HandleAsync(T integrationEvent, CancellationToken cancellationToken);
}
