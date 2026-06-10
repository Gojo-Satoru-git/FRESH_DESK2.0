using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketCustomFieldConfiguration : IEntityTypeConfiguration<TicketCustomField>
{
    public void Configure(EntityTypeBuilder<TicketCustomField> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_custom_fields_pkey");

        builder.ToTable("ticket_custom_fields", "ticket");

        builder.HasIndex(e => new { e.FieldKey, e.FieldValue }, "idx_tcf_key");

        builder.HasIndex(e => e.TicketId, "idx_tcf_ticket");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.FieldKey).HasMaxLength(80).HasColumnName("field_key");
        
        builder.Property(e => e.FieldValue).HasColumnName("field_value");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Ignore(e => e.RowVersion);

        builder.HasOne(d => d.Ticket).WithMany(p => p.TicketCustomFields).HasForeignKey(d => d.TicketId).HasConstraintName("ticket_custom_fields_ticket_id_fkey");
    }
}
