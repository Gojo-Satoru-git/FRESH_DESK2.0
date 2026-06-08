using Adrenalin.Modules.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public sealed class TicketWatcherConfiguration : IEntityTypeConfiguration<TicketWatcher>
{
    public void Configure(EntityTypeBuilder<TicketWatcher> builder)
    {
        builder.ToTable("ticket_watchers", "ticket");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(x => x.TicketId).HasColumnName("ticket_id");
        
        builder.Property(x => x.UserId).HasColumnName("user_id");
        
        builder.Property(x => x.AddedBy).HasColumnName("added_by");
        
        builder.Property(x => x.AddedAt).HasDefaultValueSql("now()").HasColumnName("added_at");
        
        builder.Property(x => x.RowVersion).HasColumnName("row_version");

        builder.Property(x => x.AddedBy).IsRequired();

        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TicketId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Ticket).WithMany(t => t.TicketWatchers).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
    }
}