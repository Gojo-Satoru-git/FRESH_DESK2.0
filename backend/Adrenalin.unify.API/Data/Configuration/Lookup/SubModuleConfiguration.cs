using Adrenalin.unify.API.Models.Lookup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Lookup;

public class SubModuleConfiguration
    : IEntityTypeConfiguration<SubModule>
{
    public void Configure(EntityTypeBuilder<SubModule> builder)
    {
        builder.ToTable(
            "sub_modules",
            "lookup");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id");

        builder.Property(x => x.Code)
            .HasColumnName("code");

        builder.Property(x => x.Label)
            .HasColumnName("label");

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.RequiresDualConfirm)
            .HasColumnName("requires_dual_confirm");

        builder.Property(x => x.RequiresComplianceReview)
            .HasColumnName("requires_compliance_review");

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

        builder.HasOne(x => x.ProductModule)
            .WithMany(x => x.SubModules)
            .HasForeignKey(x => x.ModuleId);

        builder.HasOne(x => x.CreatedByNavigation)
            .WithMany(x => x.SubModuleCreatedByNavigations)
            .HasForeignKey(x => x.CreatedBy);

        builder.HasOne(x => x.UpdatedByNavigation)
            .WithMany(x => x.SubModuleUpdatedByNavigations)
            .HasForeignKey(x => x.UpdatedBy);
    }
}