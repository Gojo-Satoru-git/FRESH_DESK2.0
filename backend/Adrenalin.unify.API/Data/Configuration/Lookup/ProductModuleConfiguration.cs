using Adrenalin.unify.API.Models.Lookup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Lookup;

public class ProductModuleConfiguration
    : IEntityTypeConfiguration<ProductModule>
{
    public void Configure(EntityTypeBuilder<ProductModule> builder)
    {
        builder.ToTable(
            "modules",
            "lookup");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.Department)
            .HasColumnName("department");

        builder.Property(x => x.IsMandatoryForClosure)
            .HasColumnName("is_mandatory_for_closure");

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

        builder.Property(x => x.ModifiedAt)
            .HasColumnName("modified_at");

        builder.Property(x => x.ModifiedBy)
            .HasColumnName("modified_by");

        builder.HasOne(x => x.CreatedByNavigation)
            .WithMany(x => x.ModuleCreatedByNavigations)
            .HasForeignKey(x => x.CreatedBy);

        builder.HasOne(x => x.UpdatedByNavigation)
            .WithMany(x => x.ModuleUpdatedByNavigations)
            .HasForeignKey(x => x.UpdatedBy);

        builder.HasMany(x => x.SubModules)
            .WithOne(x => x.ProductModule)
            .HasForeignKey(x => x.ModuleId);
    }
}