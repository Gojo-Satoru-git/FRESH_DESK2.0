using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class EscalationRuleConfiguration : IEntityTypeConfiguration<EscalationRule>
{
    public void Configure(EntityTypeBuilder<EscalationRule> builder)
    {
        builder.HasKey(e => e.Id).HasName("escalation_rules_pkey");

        builder.ToTable("escalation_rules", "sla");

        builder.HasIndex(e => new { e.IsActive, e.NoResponseMinutes }, "idx_escalation_rules_active");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            
        builder.Property(e => e.NoResponseMinutes).HasColumnName("no_response_minutes");
        
        builder.Property(e => e.NotifyRole).HasMaxLength(60).HasColumnName("notify_role");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("escalation_rules_created_by_fkey");
    }
}
