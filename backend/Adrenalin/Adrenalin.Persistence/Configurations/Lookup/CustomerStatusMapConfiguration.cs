using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class CustomerStatusMapConfiguration : IEntityTypeConfiguration<CustomerStatusMap>
{
    public void Configure(EntityTypeBuilder<CustomerStatusMap> builder)
    {
        builder.HasKey(e => e.Id).HasName("customer_status_map_pkey");

        builder.ToTable("customer_status_map", "config", tb => tb.HasComment("Maps every agent-facing ticket status to the simplified customer-facing label. Customer portal API reads this table — customers never see statuses like pending_internal or compliance_review. Only Open, Reopen, Closed are exposed."));

        builder.HasIndex(e => e.AgentStatus, "uq_customer_status_map").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.AgentStatus).HasMaxLength(40).HasColumnName("agent_status");

        builder.Property(e => e.CustomerDescription).HasMaxLength(200).HasColumnName("customer_description");

        builder.Property(e => e.CustomerLabel).HasMaxLength(40).HasColumnName("customer_label");

        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
    }
}
