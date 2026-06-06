using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.HasKey(e => e.Id).HasName("challenges_pkey");

        builder.ToTable("challenges", "gamification");

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.IsActive, e.StartsAt, e.EndsAt }, "idx_challenges_active");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.BadgeId).HasColumnName("badge_id");
        
        builder.Property(e => e.BonusPoints).HasColumnName("bonus_points");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Description).HasColumnName("description");
        
        builder.Property(e => e.EndsAt).HasColumnName("ends_at");
        
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.Metric).HasMaxLength(80).HasColumnName("metric");
            
        builder.Property(e => e.Scope).HasMaxLength(20).HasDefaultValueSql("'global'::character varying").HasColumnName("scope");
            
        builder.Property(e => e.StartsAt).HasColumnName("starts_at");
        
        builder.Property(e => e.TargetValue).HasColumnName("target_value");
        
        builder.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");

        builder.HasOne(d => d.Badge).WithMany(p => p.Challenges).HasForeignKey(d => d.BadgeId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("challenges_badge_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("challenges_created_by_fkey");

        builder.HasOne<Group>().WithMany().HasForeignKey(d => d.GroupId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("challenges_group_id_fkey");
    }
}
