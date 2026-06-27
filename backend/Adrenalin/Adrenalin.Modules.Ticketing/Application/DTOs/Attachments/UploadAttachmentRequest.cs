using Microsoft.AspNetCore.Http;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;

public sealed record UploadAttachmentRequest(
    Guid? CommentId,
    Guid UploadedBy,
    IFormFile File
);
