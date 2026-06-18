using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Persistence.Configurations.Shared;

public class IntegrationEventLogConfiguration : IEntityTypeConfiguration<IntegrationEventLog>
{
    public void Configure(EntityTypeBuilder<IntegrationEventLog> builder)
    {
        builder.ToTable("integration_event_logs", "notification");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();
    }
}
