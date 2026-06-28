// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Interfaces/IAuditLogWriter.cs
// NEW FILE

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

/// <summary>
/// FS-05 FR-RP-009 / FR-RP-036 / FR-RP-050 / FR-RP-051 / FR-RP-052 — every create, edit,
/// status-change, deletion, and permission-toggle action must produce a discrete audit
/// entry with actor, timestamp, action type, and field-level diff (old/new value).
/// Writes to audit.audit_log. Call explicitly from handlers — nothing populates this
/// table automatically today (no DB trigger, no SaveChanges interceptor wired up).
/// </summary>
public interface IAuditLogWriter
{
    Task WriteAsync(
        string tableName,
        Guid recordId,
        string changeType,
        Guid actorId,
        string? oldValues,
        string? newValues,
        CancellationToken ct = default);
}
