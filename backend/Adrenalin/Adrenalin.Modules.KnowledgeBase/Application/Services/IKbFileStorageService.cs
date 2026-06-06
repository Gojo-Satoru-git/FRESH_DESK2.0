namespace Adrenalin.Modules.KB.Application.Services;

/// <summary>
/// Abstracts how KB attachment files are persisted on the server.
/// The default implementation writes files to the local disk under
/// wwwroot/kb-attachments/{articleId}/. Swap it for a cloud implementation
/// (S3, Azure Blob, etc.) without touching any handler or controller.
/// </summary>
public interface IKbFileStorageService
{
    /// <summary>
    /// Saves the uploaded stream to persistent storage and returns
    /// the relative URL stored in <c>kb_attachments.file_url</c>.
    /// </summary>
    Task<string> SaveAsync(Guid articleId, string fileName, Stream contentStream, CancellationToken ct);

    /// <summary>
    /// Deletes a previously saved file by its stored URL.
    /// Implementations should not throw if the file no longer exists.
    /// </summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct);
}
