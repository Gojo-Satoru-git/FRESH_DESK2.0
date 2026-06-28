using System;
using System.Collections.Generic;
using System.Net;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }

    public string SchemaName { get; private set; } = null!;

    public string TableName { get; private set; } = null!;

    public Guid RecordId { get; private set; }

    public string Action { get; private set; } = null!;

    public string? OldValues { get; private set; }

    public string? NewValues { get; private set; }

    public Guid? ChangedBy { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public IPAddress? IpAddress { get; private set; }

    public Guid? SessionId { get; private set; }

    public User? ChangedByNavigation { get; private set; }

    public static AuditLog Create(
        string schemaName,
        string tableName,
        Guid recordId,
        string action,
        Guid? changedBy,
        string? oldValues,
        string? newValues)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            SchemaName = schemaName,
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            ChangedBy = changedBy,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedAt = DateTime.UtcNow,
        };
    }
}
