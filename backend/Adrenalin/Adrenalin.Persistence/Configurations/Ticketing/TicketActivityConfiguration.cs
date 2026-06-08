using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public sealed class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_activities_pkey");

        builder.ToTable("ticket_activities", "ticket");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.TicketId)
            .HasColumnName("ticket_id");

        builder.Property(e => e.ActivityType)
            .HasMaxLength(100)
            .HasColumnName("activity_type");

        builder.Property(e => e.OldValue)
            .HasColumnName("old_value");

        builder.Property(e => e.NewValue)
            .HasColumnName("new_value");

        builder.Property(e => e.PerformedBy)
            .HasColumnName("performed_by");

        builder.Property(e => e.PerformedAt)
            .HasColumnName("performed_at");

        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.TicketActivities)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ticket_activities_ticket_id_fkey");
    }
}
