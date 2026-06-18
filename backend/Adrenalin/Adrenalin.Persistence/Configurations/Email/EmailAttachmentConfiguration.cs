using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Email;

public class EmailAttachmentConfiguration : IEntityTypeConfiguration<EmailAttachment>
{
    public void Configure(EntityTypeBuilder<EmailAttachment> builder)
    {
        builder.ToTable("email_attachments", "ticket");
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Hash).HasMaxLength(100);
        builder.Property(a => a.StoragePath).HasMaxLength(1000);
    }
}
