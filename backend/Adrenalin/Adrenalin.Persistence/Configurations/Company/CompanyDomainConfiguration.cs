using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Company;

public class CompanyDomainConfiguration : IEntityTypeConfiguration<CompanyDomain>
{
    public void Configure(EntityTypeBuilder<CompanyDomain> builder)
    {
        builder.HasKey(e => e.Id).HasName("company_domains_pkey");

        builder.ToTable("company_domains", "company", tb => tb.HasComment("Email domains owned by each company. UNIQUE on lower(domain) enables O(1) auto-routing of incoming emails to the correct company account."));

        builder.HasIndex(e => e.CompanyId, "idx_company_domains_company").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Domain, "idx_company_domains_domain");

        builder.HasIndex(e => e.Domain, "uq_company_domains_domain").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        
        builder.Property(e => e.Domain).HasMaxLength(255).HasColumnName("domain");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.IsPrimary).HasColumnName("is_primary");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Ignore(e => e.RowVersion);
        builder.Ignore(e => e.IsVerified);  
        builder.Ignore(e => e.VerifiedAt);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.UpdatedBy);

        builder.HasOne(d => d.Company).WithMany(p => p.CompanyDomains).HasForeignKey(d => d.CompanyId).HasConstraintName("company_domains_company_id_fkey");
    }
}
