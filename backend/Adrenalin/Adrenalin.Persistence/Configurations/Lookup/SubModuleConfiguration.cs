using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class SubModuleConfiguration : IEntityTypeConfiguration<SubModule>
{
    public void Configure(EntityTypeBuilder<SubModule> builder)
    {
        builder.HasKey(e => e.Id)
            .HasName("sub_modules_pkey");

        builder.ToTable(
            "sub_modules", 
            "lookup", 
            tb => tb.HasComment(
                "Fine-grained ticket classification. requires_dual_confirm triggers GRAPH-006; requires_compliance_review triggers GRAPH-005. Sub-module wins over module in scope resolution."));

        builder.HasIndex(e => new 
        {   
            e.ModuleId, 
            e.IsActive 
        }, "idx_sub_modules_module_active").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.ModuleId, "idx_sub_modules_module_id");

        builder.HasIndex(e => e.Code, "uq_sub_modules_code")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasColumnName("code");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by");
        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(e => e.Label)
            .HasMaxLength(200)
            .HasColumnName("label");

        builder.Property(e => e.ModuleId)
            .HasColumnName("module_id");
        builder.Property(e => e.RequiresComplianceReview)
            .HasColumnName("requires_compliance_review");
        builder.Property(e => e.RequiresDualConfirm)
            .HasColumnName("requires_dual_confirm");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_sub_modules_created_by");

        builder.HasOne(d => d.Module).WithMany()
            .HasForeignKey(d => d.ModuleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("sub_modules_module_id_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_sub_modules_updated_by");
    }
}
