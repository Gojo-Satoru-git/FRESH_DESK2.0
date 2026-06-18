namespace Adrenalin.SharedKernel.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string fileurl,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string fileurl,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string fileurl,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> EnumerateFilesAsync(
        string folder,
        CancellationToken cancellationToken = default);
}
