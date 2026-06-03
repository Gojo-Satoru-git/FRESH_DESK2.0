using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class CustomerTierConfiguration : IEntityTypeConfiguration<CustomerTier>
{
    public void Configure(EntityTypeBuilder<CustomerTier> builder)
    {
        builder.HasKey(e => e.Code).HasName("customer_tiers_pkey");

        builder.ToTable("customer_tiers", "lookup", tb => tb.HasComment("M1-M4 customer tiers. priority_bump is used by SLA engine to elevate priority at creation."));

        builder.HasIndex(e => e.Code, "uq_customer_tiers_code").IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(10)
            .HasColumnName("code");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.Description).HasColumnName("description");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.Label)
            .HasMaxLength(80)
            .HasColumnName("label");

        builder.Property(e => e.PriorityBump).HasColumnName("priority_bump");
    }
}
