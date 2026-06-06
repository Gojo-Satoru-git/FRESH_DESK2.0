using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.KnowledgeBase.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public class PortalBannerConfiguration : IEntityTypeConfiguration<PortalBanner>
{
    public void Configure(EntityTypeBuilder<PortalBanner> builder)
    {
        builder.HasKey(e => e.Id).HasName("portal_banners_pkey");

        builder.ToTable("portal_banners", "kb");

        builder.HasIndex(e => new { e.IsActive, e.ActiveFrom, e.ActiveTo }, "idx_portal_banners_active").HasFilter("(is_active = true)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.ActiveFrom).HasColumnName("active_from");

        builder.Property(e => e.ActiveTo).HasColumnName("active_to");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");

        builder.Property(e => e.Message).HasColumnName("message");

        builder.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("portal_banners_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("portal_banners_updated_by_fkey");
    }
}
