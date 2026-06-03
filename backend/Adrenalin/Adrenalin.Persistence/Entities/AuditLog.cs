using System;
using System.Collections.Generic;
using System.Net;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Cross-schema forensic audit log. Populated by application layer or generic audit triggers. old_values/new_values store JSONB diff for immutable change history.
/// </summary>
public partial class AuditLog
{
    public Guid Id { get; set; }

    public string SchemaName { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public Guid RecordId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public Guid? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public IPAddress? IpAddress { get; set; }

    public Guid? SessionId { get; set; }

    public virtual User? ChangedByNavigation { get; set; }
}
