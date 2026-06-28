using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Agent;

public class AgentStatusHistoryConfiguration : IEntityTypeConfiguration<AgentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AgentStatusHistory> builder)
    {
        builder.ToTable("agent_status_history", "agent");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.AgentId).HasColumnName("agent_id").IsRequired();
        builder.Property(e => e.OldStatus).HasColumnName("old_status").HasMaxLength(40);
        builder.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(40).IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(e => e.ChangedBy).HasColumnName("changed_by");
        builder.Property(e => e.ChangedAt).HasColumnName("changed_at").HasDefaultValueSql("now()");

        builder.HasOne(d => d.Agent)
            .WithMany(p => p.StatusHistories)
            .HasForeignKey(d => d.AgentId);

        builder.HasIndex(e => new { e.AgentId, e.ChangedAt })
            .HasDatabaseName("idx_agent_status_history_agent");
    }
}