using Adrenalin.Infrastructure.Storage.Providers;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Adrenalin.Infrastructure.Storage;

public sealed class LocalFileStorageProvider : IFileStorageProvider
{
    public string ProviderName => "Local";

    private readonly string _basePath;

    private static readonly string[] UnsafeExtensions = new[]
    {
        ".exe", ".dll", ".bat", ".cmd", ".sh", ".py", ".pl", ".php", ".asp", ".aspx", 
        ".cshtml", ".vbhtml", ".js", ".vbs", ".msi", ".com", ".scr", ".pif", ".cpl", ".html", ".htm"
    };

    public LocalFileStorageProvider(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:LocalPath"] ?? "uploads";

        Directory.CreateDirectory(_basePath);
    }

    private void ValidatePath(string fullPath)
    {
        var canonicalBasePath = Path.GetFullPath(_basePath);
        var canonicalFullPath = Path.GetFullPath(fullPath);

        if (!canonicalFullPath.StartsWith(canonicalBasePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Access denied: invalid file path.");
        }
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken) {
        var safeFileName = Path.GetFileName(fileName);
        
        var extension = Path.GetExtension(safeFileName)?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension) && UnsafeExtensions.Contains(extension))
        {
            throw new InvalidOperationException("File extension is not allowed for security reasons.");
        }

        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;
            stream = ms;
        }

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        stream.Position = 0;
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        stream.Position = 0;

        var uniqueFileName = $"{hashString}{extension}";
        var fileurl = Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        var fullPath = Path.Combine(_basePath, fileurl);

        ValidatePath(fullPath);

        if (!File.Exists(fullPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await using var fileStream = File.Create(fullPath);
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        return fileurl;
    }

    public Task<Stream> OpenReadAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileurl);

        ValidatePath(fullPath);

        Stream stream = File.OpenRead(fullPath);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileurl);

        ValidatePath(fullPath);

        if (File.Exists(fullPath)) {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string fileurl, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileurl);

        ValidatePath(fullPath);

        return Task.FromResult(File.Exists(fullPath));
    }
    public Task<IEnumerable<string>> EnumerateFilesAsync(string folder, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        ValidatePath(folderPath);

        if (!Directory.Exists(folderPath))
            return Task.FromResult(Enumerable.Empty<string>());

        var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
        var fileUrls = files.Select(f => Path.GetRelativePath(_basePath, f).Replace("\\", "/"));

        return Task.FromResult(fileUrls);
    }
} 