using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Email;

public class EmailAliasRoutingConfiguration : IEntityTypeConfiguration<EmailAliasRouting>
{
    public void Configure(EntityTypeBuilder<EmailAliasRouting> builder)
    {
        builder.ToTable("email_alias_routes", "config");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.EmailAddress).IsUnique();
        
        builder.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
    }
}
