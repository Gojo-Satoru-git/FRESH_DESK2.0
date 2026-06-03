using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class GeoRegionConfiguration : IEntityTypeConfiguration<GeoRegion>
{
    public void Configure(EntityTypeBuilder<GeoRegion> builder)
    {
        builder.HasKey(e => e.Code).HasName("geo_regions_pkey");

        builder.ToTable("geo_regions", "lookup", tb => tb.HasComment("Region-specific business hours and timezone. SLA clock runs only within these windows."));

        builder.HasIndex(e => e.Code, "uq_geo_regions_code").IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(20)
            .HasColumnName("code");

        builder.Property(e => e.BusinessEnd).HasColumnName("business_end");
        builder.Property(e => e.BusinessStart).HasColumnName("business_start");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.Label)
            .HasMaxLength(80)
            .HasColumnName("label");

        builder.Property(e => e.Timezone)
            .HasMaxLength(60)
            .HasColumnName("timezone");

        builder.Property(e => e.WorkingDays)
            .HasMaxLength(20)
            .HasDefaultValueSql("'Mon-Fri'::character varying")
            .HasColumnName("working_days");
    }
}
