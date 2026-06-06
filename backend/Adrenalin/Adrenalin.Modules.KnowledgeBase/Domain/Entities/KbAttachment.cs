using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.KB.Domain.Entities;

/// <summary>
/// Object-storage reference attached to a KB article. Table: kb.kb_attachments
/// Schema columns: id, article_id, file_url, file_name, file_size_bytes, mime_type,
///                 is_deleted, created_at
/// Note: No is_active, updated_at, updated_by, or row_version in schema.
/// </summary>
public sealed class KbAttachment : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public string FileUrl { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public long? FileSizeBytes { get; private set; }
    public string? MimeType { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private KbAttachment() { }

    public static KbAttachment Create(Guid articleId, string fileUrl, string fileName,
        long? fileSizeBytes, string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("File URL cannot be blank.", nameof(fileUrl));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be blank.", nameof(fileName));
        if (fileName.Length > 255)
            throw new ArgumentException("File name cannot exceed 255 characters.", nameof(fileName));

        return new KbAttachment
        {
            Id = Guid.NewGuid(),
            ArticleId = articleId,
            FileUrl = fileUrl.Trim(),
            FileName = fileName.Trim(),
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType?.Trim(),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SoftDelete()
    {
        if (IsDeleted) throw new InvalidOperationException("Attachment is already deleted.");
        IsDeleted = true;
    }
}
