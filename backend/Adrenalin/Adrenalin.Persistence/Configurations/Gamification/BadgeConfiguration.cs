using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.HasKey(e => e.Id).HasName("badges_pkey");

        builder.ToTable("badges", "gamification");

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => e.Code, "uq_badges_code").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.Code).HasMaxLength(80).HasColumnName("code");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Description).HasColumnName("description");
        
        builder.Property(e => e.IconUrl).HasColumnName("icon_url");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
            
        builder.Property(e => e.PointsValue).HasColumnName("points_value");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("badges_created_by_fkey");
    }
}
