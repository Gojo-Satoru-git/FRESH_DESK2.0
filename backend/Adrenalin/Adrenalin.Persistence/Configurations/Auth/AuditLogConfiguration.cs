using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("audit_log_pkey");

        builder.ToTable("audit_log", "audit", tb => tb.HasComment("Cross-schema forensic audit log. Populated by application layer or generic audit triggers. old_values/new_values store JSONB diff for immutable change history."));

        builder.HasIndex(e => new { e.TableName, e.RecordId, e.ChangedAt }, "idx_audit_log_record").IsDescending(false, false, true);

        builder.HasIndex(e => new { e.TableName, e.ChangedAt }, "idx_audit_log_table").IsDescending(false, true);

        builder.HasIndex(e => new { e.ChangedBy, e.ChangedAt }, "idx_audit_log_user").IsDescending(false, true);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.Action).HasMaxLength(10).HasColumnName("action");

        builder.Property(e => e.ChangedAt).HasDefaultValueSql("now()").HasColumnName("changed_at");

        builder.Property(e => e.ChangedBy).HasColumnName("changed_by");

        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        builder.Property(e => e.NewValues).HasColumnType("jsonb").HasColumnName("new_values");

        builder.Property(e => e.OldValues).HasColumnType("jsonb").HasColumnName("old_values");

        builder.Property(e => e.RecordId).HasColumnName("record_id");

        builder.Property(e => e.SchemaName).HasMaxLength(60).HasColumnName("schema_name");

        builder.Property(e => e.SessionId).HasColumnName("session_id");

        builder.Property(e => e.TableName).HasMaxLength(60).HasColumnName("table_name");

        builder.HasOne(d => d.ChangedByNavigation).WithMany().HasForeignKey(d => d.ChangedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("audit_log_changed_by_fkey");
    }
}
