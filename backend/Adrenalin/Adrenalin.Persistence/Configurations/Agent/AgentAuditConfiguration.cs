using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Agent;

public class AgentAuditConfiguration : IEntityTypeConfiguration<AgentAudit>
{
    public void Configure(EntityTypeBuilder<AgentAudit> builder)
    {
        builder.ToTable("agent_audit", "agent");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.AgentId).HasColumnName("agent_id").IsRequired();
        builder.Property(e => e.Action).HasColumnName("action").HasMaxLength(60).IsRequired();
        builder.Property(e => e.FieldName).HasColumnName("field_name").HasMaxLength(100);
        builder.Property(e => e.OldValue).HasColumnName("old_value");
        builder.Property(e => e.NewValue).HasColumnName("new_value");
        builder.Property(e => e.ChangedBy).HasColumnName("changed_by");
        builder.Property(e => e.ChangedAt).HasColumnName("changed_at").HasDefaultValueSql("now()");

        builder.HasOne(d => d.Agent)
            .WithMany(p => p.AuditLogs)
            .HasForeignKey(d => d.AgentId);

        builder.HasIndex(e => new { e.AgentId, e.ChangedAt })
            .HasDatabaseName("idx_agent_audit_agent");
    }
}