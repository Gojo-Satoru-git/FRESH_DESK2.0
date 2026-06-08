using Adrenalin.Modules.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public sealed class TicketRelationConfiguration : IEntityTypeConfiguration<TicketRelation>
{
    public void Configure(EntityTypeBuilder<TicketRelation> builder)
    {
        builder.ToTable("ticket_relations", "ticket");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RelationType).HasConversion<string>().IsRequired();

        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.ParentTicketId, x.ChildTicketId, x.RelationType }).IsUnique();

        builder.HasOne<Ticket>().WithMany(t => t.TicketRelations).HasForeignKey(x => x.ParentTicketId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Ticket>().WithMany(t => t.ChildRelations).HasForeignKey(x => x.ChildTicketId).OnDelete(DeleteBehavior.Restrict);
    }
}