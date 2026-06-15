using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_attachments_pkey");

        builder.ToTable("ticket_attachments", "ticket");
        builder.HasQueryFilter(e => !e.IsDeleted && !e.Ticket.IsDeleted);

        builder.HasIndex(e => e.CommentId, "idx_ticket_attachments_comment").HasFilter("((comment_id IS NOT NULL) AND (is_deleted = false))");

        builder.HasIndex(e => e.TicketId, "idx_ticket_attachments_ticket").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CommentId).HasColumnName("comment_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.FileName).HasMaxLength(255).HasColumnName("file_name");
        
        builder.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
        
        builder.Property(e => e.FileUrl).HasColumnName("file_url");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.MimeType).HasMaxLength(100).HasColumnName("mime_type");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Ignore(e => e.RowVersion);
        
        builder.Ignore(e => e.UpdatedAt);
        
        builder.Ignore(e => e.UpdatedBy);

        builder.HasOne(d => d.Comment).WithMany(c => c.Attachments).HasForeignKey(d => d.CommentId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_attachments_comment_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_attachments_created_by_fkey");

        builder.HasOne(d => d.Ticket).WithMany(p => p.TicketAttachments).HasForeignKey(d => d.TicketId).HasConstraintName("ticket_attachments_ticket_id_fkey");
    }
}
