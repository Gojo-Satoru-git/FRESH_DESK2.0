using Adrenalin.Modules.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketWatcherConfiguration : IEntityTypeConfiguration<TicketWatcher>
{
    public void Configure(EntityTypeBuilder<TicketWatcher> builder)
    {
        builder.ToTable("ticket_watchers", "ticket");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.TicketId).HasColumnName("ticket_id");
        builder.Property(w => w.UserId).HasColumnName("user_id");
        builder.Property(w => w.AddedByUserId).HasColumnName("added_by_user_id");
        builder.Property(w => w.CreatedAt).HasColumnName("created_at");
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at");
        builder.Property(w => w.CreatedBy).HasColumnName("created_by");
        builder.Property(w => w.UpdatedBy).HasColumnName("updated_by");
        builder.Property(w => w.IsDeleted).HasColumnName("is_deleted");

        builder.HasIndex(w => w.TicketId);
        builder.HasIndex(w => w.UserId);
        builder.HasIndex(w => new { w.TicketId, w.UserId }).IsUnique().HasFilter("(is_deleted = false)");
        
        builder.HasOne<Ticket>()
            .WithMany(t => t.TicketWatchers)
            .HasForeignKey(w => w.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
