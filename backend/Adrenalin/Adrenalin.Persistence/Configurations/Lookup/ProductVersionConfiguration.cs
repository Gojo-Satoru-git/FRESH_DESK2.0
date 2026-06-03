using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class ProductVersionConfiguration : IEntityTypeConfiguration<ProductVersion>
{
    public void Configure(EntityTypeBuilder<ProductVersion> builder)
    {
        builder.HasKey(e => e.Id).HasName("product_versions_pkey");

        builder.ToTable("product_versions", "lookup", tb => tb.HasComment("Named product release lines. code must be lowercase for scope resolver. Soft-deleted versions remain on historical tickets but are hidden from new-ticket UI."));

        builder.HasIndex(e => e.IsActive, "idx_product_versions_active").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Code, "uq_product_versions_code").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.IsLegacy).HasColumnName("is_legacy");

        builder.Property(e => e.Label)
            .HasMaxLength(150)
            .HasColumnName("label");

        builder.Property(e => e.ReleaseDate).HasColumnName("release_date");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_product_versions_created_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_product_versions_updated_by");
    }
}
