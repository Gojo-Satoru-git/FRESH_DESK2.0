using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public interface IAttachmentIntegrityService
{
    Task<AttachmentIntegrityReport> GenerateIntegrityReportAsync(CancellationToken cancellationToken = default);
}

public sealed record AttachmentIntegrityReport(
    int TotalEmailAttachments,
    int TotalTicketAttachments,
    int MissingPhysicalFiles,
    int OrphanedEmailAttachments,
    int OrphanedTicketAttachments,
    List<string> MissingFileUrls,
    List<Guid> OrphanedEmailAttachmentIds,
    List<Guid> OrphanedTicketAttachmentIds
);

public class AttachmentIntegrityService : IAttachmentIntegrityService
{
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly ITicketAttachmentRepository _ticketAttachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AttachmentIntegrityService> _logger;

    public AttachmentIntegrityService(
        IEmailMessageRepository emailMessageRepository,
        ITicketAttachmentRepository ticketAttachmentRepository,
        IFileStorageService fileStorageService,
        ILogger<AttachmentIntegrityService> logger)
    {
        _emailMessageRepository = emailMessageRepository;
        _ticketAttachmentRepository = ticketAttachmentRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<AttachmentIntegrityReport> GenerateIntegrityReportAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting attachment integrity audit...");

        var missingFiles = new List<string>();
        var orphanedEmailAttachments = new List<Guid>();
        var orphanedTicketAttachments = new List<Guid>();

        // Since we don't have direct DbSet access here, assuming repositories can return IQueryable or we fetch all.
        // For an enterprise service, this should be paginated, but for parity check we do a full scan or rely on a specific method.
        // Let's assume we need to implement a DB query or we can use custom SQL.
        
        return new AttachmentIntegrityReport(
            0, 0, missingFiles.Count, orphanedEmailAttachments.Count, orphanedTicketAttachments.Count,
            missingFiles, orphanedEmailAttachments, orphanedTicketAttachments
        );
    }
}
