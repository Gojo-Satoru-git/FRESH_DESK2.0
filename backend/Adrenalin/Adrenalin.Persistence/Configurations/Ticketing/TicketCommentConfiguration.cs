using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;


namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_comments_pkey");

        builder.ToTable("ticket_comments", "ticket");

        builder.HasIndex(e => e.AuthorId, "idx_ticket_comments_author").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.TicketId, e.CreatedAt }, "idx_ticket_comments_ticket").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AuthorId).HasColumnName("author_id");
        
        builder.Property(e => e.Body).HasColumnName("body");
        
        builder.Property(e => e.ContactId).HasColumnName("contact_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.IsPrivate).HasColumnName("is_private");

        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Ignore(e => e.UpdatedBy);
        
        builder.Ignore(e => e.RowVersion);
        builder.Ignore(e => e.MentionedUsers);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AuthorId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_comments_author_id_fkey");

        builder.HasOne<Contact>().WithMany().HasForeignKey(d => d.ContactId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_comments_contact_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_comments_created_by_fkey");

        builder.HasOne(d => d.Ticket).WithMany(p => p.TicketComments).HasForeignKey(d => d.TicketId).HasConstraintName("ticket_comments_ticket_id_fkey");
    }
}
