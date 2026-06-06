namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record AttachmentDto(
    Guid Id,
    string FileName,
    string FileUrl,
    long FileSizeBytes,
    string MimeType,
    DateTimeOffset CreatedAt
);
