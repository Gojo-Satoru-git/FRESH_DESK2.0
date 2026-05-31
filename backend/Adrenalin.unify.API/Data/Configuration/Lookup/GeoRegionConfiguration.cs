using Adrenalin.unify.API.Models.Lookup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Lookup;

public class GeoRegionConfiguration
    : IEntityTypeConfiguration<GeoRegion>
{
    public void Configure(EntityTypeBuilder<GeoRegion> builder)
    {
        builder.ToTable(
            "geo_regions",
            "lookup",
            tb => tb.HasComment(
                "Region-specific business hours and timezone."));

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(20);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(100);

        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(100);

        builder.Property(x => x.BusinessStart)
            .HasColumnName("business_start");

        builder.Property(x => x.BusinessEnd)
            .HasColumnName("business_end");

        builder.Property(x => x.WorkingDays)
            .HasColumnName("working_days");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
    }
}