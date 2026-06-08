using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketAttachment : SoftDeleteEntity
{
    public Guid TicketId { get; private set; }
    public Guid? CommentId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string FileUrl { get; private set; } = null!;
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; } = null!;
    public Ticket Ticket { get; private set; } = null!;
    public TicketComment? Comment { get; private set; }

    public Guid? UploadedBy => CreatedBy;

    private TicketAttachment() { }

    public void MoveToTicket(Guid destinationTicketId)
    {
        if (destinationTicketId == Guid.Empty)
            throw new TicketDomainException("Destination ticket ID cannot be empty.");
        TicketId = destinationTicketId;
    }

    private TicketAttachment(Guid ticketId, Guid? commentId, string fileName, string fileUrl, long fileSizeBytes, string mimeType, Guid uploadedBy)
    {
        TicketId = ticketId;
        CommentId = commentId;
        FileName = fileName;
        FileUrl = fileUrl;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        CreatedBy = uploadedBy;
    }

    public static TicketAttachment Create(Guid ticketId, Guid? commentId, string fileName, string fileUrl, long fileSizeBytes, string mimeType, Guid uploadedBy)
    {
        if (ticketId == Guid.Empty)
            throw new TicketDomainException("TicketId cannot be empty.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new TicketDomainException("FileName is required.");

        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new TicketDomainException("StorageKey is required.");

        if (fileSizeBytes < 0)
            throw new TicketDomainException("File size cannot be negative.");

        if (fileSizeBytes > 50 * 1024 * 1024)
            throw new TicketDomainException("File exceeds maximum allowed size.");

        if (uploadedBy == Guid.Empty)
            throw new TicketDomainException("UploadedBy user ID cannot be empty.");

        var ticketAttachment = new TicketAttachment(
            ticketId,
            commentId,
            fileName,
            fileUrl,
            fileSizeBytes,
            mimeType,
            uploadedBy
        );

        return ticketAttachment;
    }
}