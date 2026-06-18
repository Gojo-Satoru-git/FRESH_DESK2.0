using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Infrastructure.Storage.Providers;

namespace Adrenalin.Infrastructure.Storage;

public sealed class FileStorageResolver : IFileStorageService
{
    private readonly IFileStorageProvider _activeProvider;
    private readonly ILogger<FileStorageResolver> _logger;

    public FileStorageResolver(
        IEnumerable<IFileStorageProvider> providers,
        IConfiguration configuration,
        ILogger<FileStorageResolver> logger)
    {
        _logger = logger;
        
        var activeProviderName = configuration["Storage:ActiveProvider"] ?? "Local";
        
        var provider = providers.FirstOrDefault(p => p.ProviderName.Equals(activeProviderName, StringComparison.OrdinalIgnoreCase));
        
        if (provider == null)
        {
            _logger.LogWarning("Configured ActiveProvider '{ProviderName}' not found. Falling back to 'Local'.", activeProviderName);
            provider = providers.FirstOrDefault(p => p.ProviderName.Equals("Local", StringComparison.OrdinalIgnoreCase))
                              ?? throw new InvalidOperationException("No File Storage Providers registered.");
        }
        
        _activeProvider = provider;
        
        _logger.LogInformation("FileStorageResolver initialized with active provider: {ProviderName}", _activeProvider.ProviderName);
    }

    public Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        return _activeProvider.SaveAsync(stream, fileName, folder, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        return _activeProvider.OpenReadAsync(fileurl, cancellationToken);
    }

    public Task DeleteAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        return _activeProvider.DeleteAsync(fileurl, cancellationToken);
    }

    public Task<bool> ExistsAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        return _activeProvider.ExistsAsync(fileurl, cancellationToken);
    }

    public Task<IEnumerable<string>> EnumerateFilesAsync(string folder, CancellationToken cancellationToken = default)
    {
        return _activeProvider.EnumerateFilesAsync(folder, cancellationToken);
    }
}
