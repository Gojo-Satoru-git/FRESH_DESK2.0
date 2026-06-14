using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Notification;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("notification_logs_pkey");

        builder.ToTable("notification_logs", "notification");

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.IsFailedDelivery, e.SentAt }, "idx_notification_logs_failed").HasFilter("(is_failed_delivery = true)");

        builder.HasIndex(e => e.TemplateId, "idx_notification_logs_template");

        builder.HasIndex(e => new { e.TicketId, e.SentAt }, "idx_notification_logs_ticket").IsDescending(false, true).HasFilter("(ticket_id IS NOT NULL)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.TicketNumber).HasColumnName("ticket_number").HasMaxLength(50);

        builder.Property(e => e.ErrorMessage).HasColumnName("error_message");
        
        builder.Property(e => e.IsFailedDelivery).HasColumnName("is_failed_delivery");
        
        builder.Property(e => e.RecipientEmail).HasMaxLength(255).HasColumnName("recipient_email");
            
        builder.Property(e => e.SentAt).HasDefaultValueSql("now()").HasColumnName("sent_at");
            
        builder.Property(e => e.TemplateId).HasColumnName("template_id");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.HasOne(d => d.Template).WithMany(p => p.NotificationLogs).HasForeignKey(d => d.TemplateId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("notification_logs_template_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("notification_logs_ticket_id_fkey");
    }
}
