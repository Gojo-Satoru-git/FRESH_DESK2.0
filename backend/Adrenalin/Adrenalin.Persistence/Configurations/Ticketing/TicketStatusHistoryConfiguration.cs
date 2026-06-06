using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_status_history_pkey");

        builder.ToTable("ticket_status_history", "ticket", tb => tb.HasComment("Immutable audit log of every status transition. Never update or delete rows. Append-only; enables full status trail and SLA clock reconstruction."));

        builder.HasIndex(e => new { e.TicketId, e.ChangedAt }, "idx_tsh_ticket_time").IsDescending(false, true);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.ChangedAt).HasDefaultValueSql("now()").HasColumnName("changed_at");
        
        builder.Property(e => e.ChangedBy).HasColumnName("changed_by");
        
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(40).HasColumnName("from_status");
        
        builder.Property(e => e.Reason).HasColumnName("reason");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(40).HasColumnName("to_status");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.ChangedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_history_changed_by_fkey");

        builder.HasOne(d => d.Ticket).WithMany(p => p.TicketStatusHistories).HasForeignKey(d => d.TicketId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("ticket_status_history_ticket_id_fkey");
    }
}
