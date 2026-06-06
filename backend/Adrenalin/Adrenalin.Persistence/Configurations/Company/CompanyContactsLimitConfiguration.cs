using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Company;

public class CompanyContactsLimitConfiguration : IEntityTypeConfiguration<CompanyContactsLimit>
{
    public void Configure(EntityTypeBuilder<CompanyContactsLimit> builder)
    {
        builder.HasKey(e => e.Id).HasName("company_contacts_limit_pkey");

        builder.ToTable("company_contacts_limit", "company");

        builder.HasIndex(e => e.CompanyId, "uq_company_contacts_limit_company").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.MaxContacts).HasDefaultValue(10).HasColumnName("max_contacts");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.HasOne(d => d.Company).WithOne(p => p.CompanyContactsLimit).HasForeignKey<CompanyContactsLimit>(d => d.CompanyId).HasConstraintName("company_contacts_limit_company_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("company_contacts_limit_created_by_fkey");
    }
}
