using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Email;

public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.ToTable("email_messages", "ticket");
        
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.MessageId).IsUnique();
        builder.HasIndex(e => e.InternetMessageId).IsUnique();
        builder.HasIndex(e => e.ThreadId);
        builder.HasIndex(e => e.TicketId);
        builder.HasIndex(e => e.ProcessingState);
        builder.HasIndex(e => e.ReceivedAt);

        builder.Property(e => e.Provider).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MessageId).IsRequired().HasMaxLength(500);
        builder.Property(e => e.InternetMessageId).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ThreadId).HasMaxLength(500);
        builder.Property(e => e.InReplyTo).HasMaxLength(500);
        
        builder.Property(e => e.SenderEmail).IsRequired().HasMaxLength(255);
        builder.Property(e => e.SenderName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
        
        builder.Property(e => e.ProcessingState).HasConversion<string>().HasMaxLength(50);
        
        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.EmailMessage)
            .HasForeignKey(a => a.EmailMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
