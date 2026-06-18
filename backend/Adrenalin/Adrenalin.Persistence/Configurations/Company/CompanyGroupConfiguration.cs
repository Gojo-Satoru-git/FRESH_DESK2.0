using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Company;

public class CompanyGroupConfiguration : IEntityTypeConfiguration<CompanyGroup>
{
    public void Configure(EntityTypeBuilder<CompanyGroup> builder)
    {
        builder.HasKey(e => e.Id).HasName("company_groups_pkey");

        builder.ToTable("company_groups", "company", tb =>
            tb.HasComment("Maps companies to support groups (M:N). A company may be served by multiple groups."));

        builder.HasQueryFilter(e => !e.IsDeleted);

        // Unique: one mapping per company+group pair (including soft-deleted for restore pattern)
        builder.HasIndex(e => new { e.CompanyId, e.GroupId }, "uq_company_groups")
            .IsUnique();

        builder.HasIndex(e => e.CompanyId, "idx_company_groups_company")
            .HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.GroupId, "idx_company_groups_group")
            .HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.CompanyId, e.IsDefault }, "idx_company_groups_default")
            .HasFilter("((is_default = true) AND (is_deleted = false))");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.GroupId).HasColumnName("group_id");

        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false)
            .HasColumnName("is_default");

        builder.Property(e => e.Priority)
            .HasDefaultValue(0)
            .HasColumnName("priority");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.Ignore(e => e.RowVersion);

        // FK → company.companies
        builder.HasOne<Adrenalin.Modules.Company.Domain.Entities.Company>().WithMany()
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("company_groups_company_id_fkey");

        // FK → auth.groups
        builder.HasOne<Group>().WithMany()
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("company_groups_group_id_fkey");

        // FK → created_by user
        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("company_groups_created_by_fkey");

        // FK → updated_by user
        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("company_groups_updated_by_fkey");
    }
}
