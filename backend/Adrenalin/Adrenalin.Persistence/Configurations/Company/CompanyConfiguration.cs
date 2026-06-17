using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Company;

public class CompanyConfiguration : IEntityTypeConfiguration<Adrenalin.Modules.Company.Domain.Entities.Company>
{
    public void Configure(EntityTypeBuilder<Adrenalin.Modules.Company.Domain.Entities.Company> builder)
    {
        builder.HasKey(e => e.Id).HasName("companies_pkey");

        builder.ToTable("companies", "company", tb => tb.HasComment("Core customer account. health_score + customer_tiers.priority_bump elevates ticket priority. delivery_support_active routes tickets to delivery (hypercare). payment_on_hold triggers auto On Hold status for new tickets."));

        builder.HasIndex(e => e.GeoRegion, "idx_companies_geo_region").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.HealthScore, "idx_companies_health_score").IsDescending().HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Name, "idx_companies_name_trgm").HasFilter("(is_deleted = false)").HasMethod("gin").HasOperators(new[] { "gin_trgm_ops" });

        builder.HasIndex(e => e.PaymentOnHold, "idx_companies_payment_hold").HasFilter("((payment_on_hold = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.SupportTier, "idx_companies_tier").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.CspId, "uq_companies_csp_id").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CamUserId).HasColumnName("cam_user_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
                
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.CspId).HasMaxLength(100).HasColumnName("csp_id");
        
        builder.Property(e => e.DeliveryManagerId).HasColumnName("delivery_manager_id");
        
        builder.Property(e => e.DeliverySupportActive).HasColumnName("delivery_support_active");
        
        builder.Property(e => e.GeoRegion).HasMaxLength(20).HasColumnName("geo_region");
        
        builder.Property(e => e.HealthScore).HasDefaultValue(75).HasColumnName("health_score");
        
        builder.Property(e => e.Industry).HasMaxLength(100).HasColumnName("industry");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.IsPayrollCustomer).HasColumnName("is_payroll_customer");
        
        builder.Property(e => e.LeaveCreditCycle).HasMaxLength(30).HasColumnName("leave_credit_cycle");
        
        builder.Property(e => e.MigrationDate).HasColumnName("migration_date");
        
        builder.Property(e => e.UpdatedAt).HasColumnName("modified_at");
                
        builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");
        
        builder.Property(e => e.Notes).HasColumnName("notes");
        
        builder.Property(e => e.PaymentOnHold).HasColumnName("payment_on_hold");
        
        builder.Property(e => e.SupportTier).HasMaxLength(10).HasColumnName("support_tier");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        
        builder.Ignore(e => e.RowVersion);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CamUserId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("companies_cam_user_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("companies_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.DeliveryManagerId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("companies_delivery_manager_id_fkey");

        builder.HasOne<GeoRegion>().WithMany().HasForeignKey(d => d.GeoRegion).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("companies_geo_region_fkey");

        builder.HasOne<CustomerTier>().WithMany().HasForeignKey(d => d.SupportTier).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("companies_support_tier_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("companies_updated_by_fkey");
    }
}
