using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class PointRuleConfiguration : IEntityTypeConfiguration<PointRule>
{
    public void Configure(EntityTypeBuilder<PointRule> builder)
    {
        builder.HasKey(e => e.Id).HasName("point_rules_pkey");

        builder.ToTable("point_rules", "gamification", tb => tb.HasComment("Defines what actions earn points. max_per_day prevents farming: engine checks SUM(points) WHERE agent_id=X AND rule_id=Y AND earned_at::date = TODAY. Negative points allowed for reversals."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => e.Code, "uq_point_rules_code").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.Code).HasMaxLength(80).HasColumnName("code");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Description).HasColumnName("description");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.MaxPerDay).HasColumnName("max_per_day");
        
        builder.Property(e => e.Points).HasColumnName("points");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("point_rules_created_by_fkey");
    }
}
