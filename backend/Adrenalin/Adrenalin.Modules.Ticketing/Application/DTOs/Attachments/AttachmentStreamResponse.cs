using System.IO;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;

public sealed record AttachmentStreamResponse(
    Stream Stream,
    string FileName,
    string ContentType
);
