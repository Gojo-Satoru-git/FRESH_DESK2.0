using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.HasKey(e => e.Id).HasName("automation_rules_pkey");

        builder.ToTable("automation_rules", "sla", tb => tb.HasComment("JSONB conditions/actions allows rule engine to evolve without schema migrations. Examples: route to group on creation, reopen on reply, auto-close at day 15, escalate on no-response, set payment hold status. execution_order determines eval sequence."));

        builder.HasIndex(e => e.Actions, "idx_automation_rules_actions").HasMethod("gin");

        builder.HasIndex(e => e.Conditions, "idx_automation_rules_conditions").HasMethod("gin");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.Actions).HasDefaultValueSql("'[]'::jsonb").HasColumnType("jsonb").HasColumnName("actions");
            
        builder.Property(e => e.Conditions).HasDefaultValueSql("'[]'::jsonb").HasColumnType("jsonb").HasColumnName("conditions");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.ExecutionOrder).HasDefaultValue(100).HasColumnName("execution_order");
            
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
            
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
            
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("automation_rules_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("automation_rules_updated_by_fkey");
    }
}
