using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Email;

public class ProcessedEmailLogConfiguration : IEntityTypeConfiguration<ProcessedEmailLog>
{
    public void Configure(EntityTypeBuilder<ProcessedEmailLog> builder)
    {
        builder.ToTable("processed_email_logs", "ticket");
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => p.InternetMessageId).IsUnique();
        
        builder.Property(p => p.InternetMessageId).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Provider).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
    }
}
