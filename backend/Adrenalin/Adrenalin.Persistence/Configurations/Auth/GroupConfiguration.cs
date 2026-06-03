using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(e => e.Id).HasName("groups_pkey");

        builder.ToTable("groups", "auth", tb => tb.HasComment("13 geo/tier groups. region_code and tier_code determine SLA policy and routing scope. Admin can create additional groups as org scales."));

        builder.HasIndex(e => e.RegionCode, "idx_groups_region").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.TierCode, "idx_groups_tier").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(e => e.RegionCode)
            .HasMaxLength(20)
            .HasColumnName("region_code");

        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.Property(e => e.TierCode)
            .HasMaxLength(10)
            .HasColumnName("tier_code");

        builder.Property(e => e.UnattendedAlertMinutes)
            .HasDefaultValue(30)
            .HasComment("Minutes after which an unassigned ticket in this group triggers the GROUP_UNATTENDED notification to group leads. Default 30 minutes. Automation rule reads this value.")
            .HasColumnName("unattended_alert_minutes");

        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("groups_created_by_fkey");

        builder.HasOne<GeoRegion>().WithMany()
            .HasForeignKey(d => d.RegionCode)
            .HasConstraintName("groups_region_code_fkey");

        builder.HasOne<CustomerTier>().WithMany()
            .HasForeignKey(d => d.TierCode)
            .HasConstraintName("groups_tier_code_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("groups_updated_by_fkey");
    }
}
