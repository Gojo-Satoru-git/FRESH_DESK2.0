using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(e => e.Id).HasName("modules_pkey");

        builder.ToTable("modules", "lookup", tb => tb.HasComment("Top-level product functional areas. is_mandatory_for_closure enforces module selection before ticket resolution. department drives default graph scope lookup."));

        builder.HasIndex(e => e.IsActive, "idx_modules_active").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Department, "idx_modules_department").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Code, "uq_modules_code").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.Code)
            .HasMaxLength(80)
            .HasColumnName("code");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.Department)
            .HasMaxLength(80)
            .HasColumnName("department");

        builder.Property(e => e.Description).HasColumnName("description");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.IsMandatoryForClosure)
            .HasDefaultValue(true)
            .HasColumnName("is_mandatory_for_closure");

        builder.Property(e => e.Label)
            .HasMaxLength(150)
            .HasColumnName("label");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_modules_created_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_modules_updated_by");

        builder.Ignore(e => e.RowVersion);
    }
}
