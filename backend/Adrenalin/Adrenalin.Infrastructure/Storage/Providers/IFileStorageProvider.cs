using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Storage.Providers;

public interface IFileStorageProvider : IFileStorageService
{
    string ProviderName { get; }
}
