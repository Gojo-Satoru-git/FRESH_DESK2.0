using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class BusinessHourConfiguration : IEntityTypeConfiguration<BusinessHour>
{
    public void Configure(EntityTypeBuilder<BusinessHour> builder)
    {
        builder.HasKey(e => e.Id).HasName("business_hours_pkey");

        builder.ToTable("business_hours", "sla");

        builder.HasIndex(e => e.GeoRegion, "idx_business_hours_region").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.EndTime).HasColumnName("end_time");
        
        builder.Property(e => e.GeoRegion).HasMaxLength(20).HasColumnName("geo_region");
            
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.StartTime).HasColumnName("start_time");
        
        builder.Property(e => e.WorkingDays).HasMaxLength(20).HasDefaultValueSql("'Mon-Fri'::character varying").HasColumnName("working_days");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("business_hours_created_by_fkey");

        builder.HasOne<GeoRegion>().WithMany().HasForeignKey(d => d.GeoRegion).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("business_hours_geo_region_fkey");
    }
}
