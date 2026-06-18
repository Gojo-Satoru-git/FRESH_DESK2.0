using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Persistence.Services;

public sealed class AttachmentIntegrityService : IAttachmentIntegrityService
{
    private readonly AdrenalinDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AttachmentIntegrityService> _logger;

    public AttachmentIntegrityService(
        AdrenalinDbContext dbContext,
        IFileStorageService fileStorageService,
        ILogger<AttachmentIntegrityService> logger)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task RunIntegrityCheckAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Attachment Integrity Check");

        // 1. Get all DB references
        var ticketAttachments = await _dbContext.TicketAttachments
            .Where(a => a.FileUrl != null)
            .Select(a => a.FileUrl)
            .ToListAsync(cancellationToken);

        var kbAttachments = await _dbContext.KbAttachments
            .Where(a => a.FileUrl != null)
            .Select(a => a.FileUrl)
            .ToListAsync(cancellationToken);

        var emailAttachments = await _dbContext.EmailAttachments
            .Where(a => a.StoragePath != null)
            .Select(a => a.StoragePath!)
            .ToListAsync(cancellationToken);

        var allDbPaths = ticketAttachments
            .Concat(kbAttachments)
            .Concat(emailAttachments)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .Select(p => p.Replace("\\", "/"))
            .ToHashSet();

        // 2. Check for missing files on disk
        foreach (var dbPath in allDbPaths)
        {
            if (!await _fileStorageService.ExistsAsync(dbPath, cancellationToken))
            {
                _logger.LogWarning("Integrity Violation: DB points to missing physical file: {DbPath}", dbPath);
            }
        }

        // 3. Find orphaned physical files
        var physicalFiles = await _fileStorageService.EnumerateFilesAsync("attachments", cancellationToken);
        foreach (var file in physicalFiles)
        {
            var normalizedFile = file.Replace("\\", "/");
            if (!allDbPaths.Contains(normalizedFile))
            {
                _logger.LogWarning("Integrity Violation: Orphaned physical file found without DB reference: {File}", normalizedFile);
                // Optionally delete them:
                // await _fileStorageService.DeleteAsync(normalizedFile, cancellationToken);
            }
        }

        _logger.LogInformation("Completed Attachment Integrity Check");
    }
}
