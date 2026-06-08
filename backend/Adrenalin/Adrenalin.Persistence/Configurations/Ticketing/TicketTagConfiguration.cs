using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public sealed class TicketTagConfiguration : IEntityTypeConfiguration<TicketTag>
{
    public void Configure(EntityTypeBuilder<TicketTag> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_tags_pkey");

        builder.ToTable("ticket_tags", "ticket");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.TicketId)
            .HasColumnName("ticket_id");

        builder.Property(e => e.TagName)
            .HasMaxLength(50)
            .HasColumnName("tag_name");

        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.TicketTags)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ticket_tags_ticket_id_fkey");
    }
}
