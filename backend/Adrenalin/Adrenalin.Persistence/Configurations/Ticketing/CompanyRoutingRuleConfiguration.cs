using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class CompanyRoutingRuleConfiguration : IEntityTypeConfiguration<CompanyRoutingRule>
{
    public void Configure(EntityTypeBuilder<CompanyRoutingRule> builder)
    {
        builder.HasKey(e => e.Id).HasName("company_routing_rules_pkey");

        builder.ToTable("company_routing_rules", "ticket", tb =>
            tb.HasComment("Defines routing rules that map ticket attributes for a company to a target group. Evaluated in rule_priority order."));

        builder.HasQueryFilter(e => !e.IsDeleted);

        // Composite index for fast rule lookup during routing
        builder.HasIndex(e => new { e.CompanyId, e.ModuleId, e.RegionCode }, "idx_routing_rules_company_module_region")
            .HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.CompanyId, "idx_routing_rules_company")
            .HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.CompanyId, e.IsDefault }, "idx_routing_rules_company_default")
            .HasFilter("((is_default = true) AND (is_deleted = false))");

        builder.HasIndex(e => new { e.CompanyId, e.RulePriority }, "idx_routing_rules_priority")
            .HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        builder.Property(e => e.ModuleId).HasColumnName("module_id");

        builder.Property(e => e.RegionCode)
            .HasMaxLength(20)
            .HasColumnName("region_code");

        builder.Property(e => e.TierCode)
            .HasMaxLength(10)
            .HasColumnName("tier_code");

        builder.Property(e => e.Priority).HasColumnName("priority_filter");
        builder.Property(e => e.TicketType).HasColumnName("ticket_type_filter");

        builder.Property(e => e.Keywords)
            .HasMaxLength(500)
            .HasColumnName("keywords");

        builder.Property(e => e.RulePriority)
            .HasDefaultValue(100)
            .HasComment("Evaluation order. Lower values are evaluated first. First fully-matching rule wins.")
            .HasColumnName("rule_priority");

        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false)
            .HasColumnName("is_default");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.Ignore(e => e.RowVersion);

        // FK → company.companies
        builder.HasOne<Adrenalin.Modules.Company.Domain.Entities.Company>().WithMany()
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("routing_rules_company_id_fkey");

        // FK → auth.groups
        builder.HasOne<Group>().WithMany()
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("routing_rules_group_id_fkey");
    }
}
