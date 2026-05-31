using Adrenalin.unify.API.Models.Lookup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Lookup;

public class CustomerTierConfiguration
    : IEntityTypeConfiguration<CustomerTier>
{
    public void Configure(EntityTypeBuilder<CustomerTier> builder)
    {
        builder.ToTable(
            "customer_tiers",
            "lookup",
            tb => tb.HasComment(
                "M1-M4 customer tiers. PriorityBump is used by SLA engine."));

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(20);

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.PriorityBump)
            .HasColumnName("priority_bump");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
    }
}