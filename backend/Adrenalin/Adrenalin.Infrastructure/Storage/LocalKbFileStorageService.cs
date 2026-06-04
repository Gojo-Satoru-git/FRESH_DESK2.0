using Adrenalin.Modules.KB.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Infrastructure.Storage;

/// <summary>
/// Stores KB attachment files on the local server disk.
///
/// Base folder is read from appsettings.json:
///   "KbStorage": { "BasePath": "C:\\uploads\\kb" }
///
/// Files are saved as:
///   {BasePath}\\{articleId}\\{originalName}_{guid}.ext
///
/// The stored URL is server-relative:
///   /kb-attachments/{articleId}/{fileName}
///
/// Add  app.UseStaticFiles(new StaticFileOptions { ... })  in Program.cs
/// if you want to serve files directly from this folder over HTTP.
/// Or replace this class with a cloud implementation (S3, Azure Blob)
/// by swapping the registration in KbServiceRegistration.
/// </summary>
public sealed class LocalKbFileStorageService : IKbFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalKbFileStorageService> _logger;

    public LocalKbFileStorageService(
        IConfiguration configuration,
        ILogger<LocalKbFileStorageService> logger)
    {
        _logger   = logger;
        _basePath = configuration["KbStorage:BasePath"]
                    ?? Path.Combine(AppContext.BaseDirectory, "kb-attachments");
    }

    /// <inheritdoc/>
    public async Task<string> SaveAsync(
        Guid articleId,
        string fileName,
        Stream contentStream,
        CancellationToken ct)
    {
        // Sanitise filename — prevent path traversal
        var safeName  = Path.GetFileNameWithoutExtension(fileName)
                            .Replace(" ", "_")
                            .Replace("..", "")
                            .Replace("/",  "")
                            .Replace("\\", "");
        var extension  = Path.GetExtension(fileName);
        var uniqueName = $"{safeName}_{Guid.NewGuid():N}{extension}";

        // Physical path: {BasePath}/{articleId}/{uniqueName}
        var directory = Path.Combine(_basePath, articleId.ToString());
        Directory.CreateDirectory(directory);

        var physicalPath = Path.Combine(directory, uniqueName);

        await using var fs = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81_920,
            useAsync: true);

        await contentStream.CopyToAsync(fs, ct);

        _logger.LogInformation(
            "KB attachment saved: article={ArticleId} path={PhysicalPath}",
            articleId, physicalPath);

        // Return a server-relative URL
        return $"/kb-attachments/{articleId}/{uniqueName}";
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileUrl, CancellationToken ct)
    {
        try
        {
            // fileUrl: /kb-attachments/{articleId}/{fileName}
            var parts    = fileUrl.TrimStart('/').Split('/');   // ["kb-attachments", articleId, fileName]
            if (parts.Length >= 3)
            {
                var physicalPath = Path.Combine(_basePath, parts[1], parts[2]);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("KB attachment deleted: {PhysicalPath}", physicalPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete KB attachment at {FileUrl}", fileUrl);
        }
        return Task.CompletedTask;
    }
}
