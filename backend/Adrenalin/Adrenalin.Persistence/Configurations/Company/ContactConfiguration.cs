using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Company;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(e => e.Id).HasName("contacts_pkey");

        builder.ToTable("contacts", "company", tb => tb.HasComment("Authorized contacts per company (5-20 per account). is_authorized=false blocks ticket creation. auto_created=true when system creates contact from unknown inbound email domain match."));

        builder.HasIndex(e => new { e.CompanyId, e.IsAuthorized }, "idx_contacts_authorized").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.CompanyId, "idx_contacts_company").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AutoCreated).HasColumnName("auto_created");
       
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
        
        builder.Property(e => e.IsAuthorized).HasDefaultValue(true).HasColumnName("is_authorized");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");
        
        builder.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
        
        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");
        
        builder.Property(e => e.Phone).HasMaxLength(30).HasColumnName("phone");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        
        builder.Property(e => e.UserId).HasColumnName("user_id");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.HasOne(d => d.Company).WithMany(p => p.Contacts).HasForeignKey(d => d.CompanyId).HasConstraintName("contacts_company_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("contacts_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("contacts_updated_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("contacts_user_id_fkey");
    }
}
