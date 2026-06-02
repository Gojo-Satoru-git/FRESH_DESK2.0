using Adrenalin.Modules.Lookup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Lookup;

public class ProductVersionConfiguration
    : IEntityTypeConfiguration<ProductVersion>
{
    public void Configure(EntityTypeBuilder<ProductVersion> builder)
    {
        builder.ToTable(
            "product_versions",
            "lookup");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Code)
            .HasColumnName("code");

        builder.Property(x => x.Label)
            .HasColumnName("label");

        builder.Property(x => x.ReleaseDate)
            .HasColumnName("release_date");

        builder.Property(x => x.IsLegacy)
            .HasColumnName("is_legacy");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.CreatedByNavigation)
            .WithMany(x => x.ProductVersionCreatedByNavigations)
            .HasForeignKey(x => x.CreatedBy);

        builder.HasOne(x => x.UpdatedByNavigation)
            .WithMany(x => x.ProductVersionUpdatedByNavigations)
            .HasForeignKey(x => x.UpdatedBy);
    }
}