// FILE: Adrenalin/Adrenalin.Persistence/Repositories/Auth/AuditLogWriter.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories.Auth;

/// <summary>
/// FR-RP-009 / FR-RP-036 / FR-RP-050-052 — writes a discrete audit.audit_log row per change.
/// Each call is its own SaveChanges so a failed audit write never silently no-ops
/// (NFR-RP-004 — "cannot be batch-hidden; each is a discrete audit entry").
/// </summary>
public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly AdrenalinDbContext _db;
    public AuditLogWriter(AdrenalinDbContext db) => _db = db;

    public async Task WriteAsync(
        string tableName,
        Guid recordId,
        string changeType,
        Guid actorId,
        string? oldValues,
        string? newValues,
        CancellationToken ct = default)
    {
        var entry = AuditLog.Create(
            schemaName: "auth",
            tableName: tableName,
            recordId: recordId,
            action: changeType,
            changedBy: actorId,
            oldValues: oldValues,
            newValues: newValues);

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(ct);
    }
}
