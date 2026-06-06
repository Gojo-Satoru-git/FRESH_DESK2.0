using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(e => e.Id).HasName("holidays_pkey");

        builder.ToTable("holidays", "sla");

        builder.HasIndex(e => new { e.GeoRegion, e.HolidayDate }, "idx_holidays_region_date").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.GeoRegion, e.HolidayDate }, "uq_holidays").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.GeoRegion).HasMaxLength(20).HasColumnName("geo_region");
            
        builder.Property(e => e.HolidayDate).HasColumnName("holiday_date");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("holidays_created_by_fkey");

        builder.HasOne<GeoRegion>().WithMany().HasForeignKey(d => d.GeoRegion).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("holidays_geo_region_fkey");
    }
}
