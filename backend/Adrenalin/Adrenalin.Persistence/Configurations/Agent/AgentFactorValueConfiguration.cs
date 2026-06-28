using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 
using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Agent;

public class AgentFactorValueConfiguration : IEntityTypeConfiguration<AgentFactorValue>
{
    public void Configure(EntityTypeBuilder<AgentFactorValue> builder)
    {
        builder.ToTable("agent_factor_values", "agent");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.AgentId).HasColumnName("agent_id").IsRequired();
        builder.Property(e => e.FactorMasterId).HasColumnName("factor_master_id").IsRequired();
        builder.Property(e => e.FactorValueId).HasColumnName("factor_value_id").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne(d => d.Agent)
            .WithMany(p => p.FactorValues)
            .HasForeignKey(d => d.AgentId);

        builder.HasIndex(e => new { e.AgentId, e.FactorMasterId, e.FactorValueId })
            .IsUnique()
            .HasDatabaseName("uq_agent_factor_values")
            .HasFilter("is_deleted = false");
    }
}