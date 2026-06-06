using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Notification;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(e => e.Id).HasName("notification_templates_pkey");

        builder.ToTable("notification_templates", "notification", tb => tb.HasComment("Handlebars templates for all notification events. code is the stable reference key. Examples: TICKET_CREATED, AGENT_REPLY, CSAT_SURVEY, SLA_BREACH, BADGE_AWARDED."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => e.Code, "uq_notification_templates_code").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.BodyHtml).HasColumnName("body_html");
        
        builder.Property(e => e.Code).HasMaxLength(80).HasColumnName("code");
            
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
            
        builder.Property(e => e.Subject).HasMaxLength(300).HasColumnName("subject");
            
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
            
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("notification_templates_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("notification_templates_updated_by_fkey");
    }
}
