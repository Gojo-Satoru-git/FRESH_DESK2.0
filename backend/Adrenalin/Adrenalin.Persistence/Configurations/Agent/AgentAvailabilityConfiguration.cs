using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Agent;

public class AgentAvailabilityConfiguration : IEntityTypeConfiguration<AgentAvailability>
{
    public void Configure(EntityTypeBuilder<AgentAvailability> builder)
    {
        builder.ToTable("agent_availability", "agent");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.AgentId).HasColumnName("agent_id").IsRequired();

        // Maps strictly to the raw column value. The DB handles constraint tracking.
        builder.Property(e => e.ShiftFactorValueId).HasColumnName("shift_factor_value_id");

        builder.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(80);
        builder.Property(e => e.ShiftStart).HasColumnName("shift_start");
        builder.Property(e => e.ShiftEnd).HasColumnName("shift_end");
        builder.Property(e => e.WorkingDays).HasColumnName("working_days").HasColumnType("jsonb");
        builder.Property(e => e.OnCallOverride).HasColumnName("on_call_override").HasDefaultValue(false);
        builder.Property(e => e.OnCallExpiry).HasColumnName("on_call_expiry");
        builder.Property(e => e.OutOfOffice).HasColumnName("out_of_office").HasDefaultValue(false);
        builder.Property(e => e.OutOfOfficeStart).HasColumnName("out_of_office_start");
        builder.Property(e => e.OutOfOfficeEnd).HasColumnName("out_of_office_end");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(d => d.Agent)
            .WithOne(p => p.Availability)
            .HasForeignKey<AgentAvailability>(d => d.AgentId);
    }
}