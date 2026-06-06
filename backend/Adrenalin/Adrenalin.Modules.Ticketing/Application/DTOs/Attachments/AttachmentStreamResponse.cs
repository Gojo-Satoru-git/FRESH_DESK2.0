using System.IO;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record AttachmentStreamResponse(
    Stream Stream,
    string FileName,
    string ContentType
);
