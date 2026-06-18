using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.SharedKernel.Interfaces;

public interface IAttachmentIntegrityService
{
    Task RunIntegrityCheckAsync(CancellationToken cancellationToken = default);
}
