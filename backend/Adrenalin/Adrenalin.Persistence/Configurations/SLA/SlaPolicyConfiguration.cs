using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.HasKey(e => e.Id).HasName("sla_policies_pkey");

        builder.ToTable("sla_policies", "sla", tb => tb.HasComment("One row per (geo_region × tier × priority). First response: Urgent=120m, High=240m, Med=360m, Low=480m. Resolution: Low=2880m (48 biz hrs). SLA minutes are business-hours only, not wall-clock."));

        builder.HasIndex(e => new { e.GeoRegion, e.TierCode, e.Priority }, "idx_sla_policies_lookup").HasFilter("(is_active = true)");

        builder.HasIndex(e => new { e.GeoRegion, e.TierCode, e.Priority }, "uq_sla_policies").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.FirstResponseMinutes).HasColumnName("first_response_minutes");
        
        builder.Property(e => e.GeoRegion).HasMaxLength(20).HasColumnName("geo_region");
            
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            
        builder.Property(e => e.Priority).HasMaxLength(20).HasColumnName("priority");
            
        builder.Property(e => e.ResolutionMinutes).HasColumnName("resolution_minutes");
        
        builder.Property(e => e.TierCode).HasMaxLength(10).HasColumnName("tier_code");
            
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
            
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("sla_policies_created_by_fkey");

        builder.HasOne<GeoRegion>().WithMany().HasForeignKey(d => d.GeoRegion).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("sla_policies_geo_region_fkey");

        builder.HasOne<CustomerTier>().WithMany().HasForeignKey(d => d.TierCode).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("sla_policies_tier_code_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("sla_policies_updated_by_fkey");
    }
}
