using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;


namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(e => e.Id).HasName("tickets_pkey");

        builder.ToTable("tickets", "ticket", tb => tb.HasComment("Central transactional entity. graph_id is resolved once at creation via scope engine. version_id/module_id/sub_module_id are normalized FK refs per Addendum v7. sla_excluded=true during hypercare/delivery mode. is_on_hold_payment for payment holds."));

        builder.HasIndex(e => new { e.IsAutoResolved, e.CreatedAt }, "idx_tickets_auto_resolved").HasFilter("((is_auto_resolved = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.CreatedAt, "idx_tickets_created_at").IsDescending().HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.ForceP1, "idx_tickets_force_p1").HasFilter("((force_p1 = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.IsOnHoldPayment, "idx_tickets_payment_hold").HasFilter("((is_on_hold_payment = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.PriorityScore, "idx_tickets_priority_score").IsDescending().HasFilter("(is_deleted = false)").HasNullSortOrder(new[] { NullSortOrder.NullsLast });

        builder.HasIndex(e => e.SolutionTypeId, "idx_tickets_solution_type").HasFilter("((solution_type_id IS NOT NULL) AND (is_deleted = false))");

        builder.HasIndex(e => e.TicketNumber, "idx_tickets_ticket_number").IsUnique();

        builder.HasIndex(e => e.VersionId, "idx_tickets_version").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.TicketNumber, "uq_tickets_number").IsUnique();

        builder.HasIndex(e => e.Status, "idx_tickets_status");

        builder.HasIndex(e => e.CompanyId, "idx_tickets_company_id");

        builder.HasIndex(e => e.AssignedAgentId, "idx_tickets_assigned_agent_id");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AssignedAgentId).HasColumnName("assigned_agent_id");
        
        builder.Property(e => e.AuditNotes).HasColumnName("audit_notes");
        
        builder.Property(e => e.AuditedBy).HasColumnName("audited_by");
        
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        
        builder.Property(e => e.ContactId).HasColumnName("contact_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        
        builder.Property(e => e.CustomerCallTaken).HasColumnName("customer_call_taken");
        
        builder.Property(e => e.CustomerReplyCount).HasComment("Incremented by automation rule on every inbound customer comment. Re-evaluation loop: every increment triggers risk agent re-score. Multiple follow-ups (≥3) increase urgency_score by +1 in the scoring formula.").HasColumnName("customer_reply_count");
        
        builder.Property(e => e.Description).HasColumnName("description");
        
        builder.Property(e => e.FixType).HasMaxLength(20).HasColumnName("fix_type");
        
        builder.Property(e => e.ForceP1).HasComment("TRUE when a force-P1 rule fires: system_down, security_breach, or sla_breach_imminent. Overrides the computed priority_score regardless of value.").HasColumnName("force_p1");
        
        builder.Property(e => e.GraphId).HasColumnName("graph_id");
        
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        
        builder.Property(e => e.ImpactScore).HasPrecision(4, 2).HasColumnName("impact_score");
        
        builder.Property(e => e.IsAutoResolved).HasComment("TRUE when the auto-resolution engine closed this ticket without human intervention. Used for KPI: target is 30–40% of total tickets. Set by auto-resolution engine after confidence > 0.85 match and solution sent.").HasColumnName("is_auto_resolved");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.IsOnHoldPayment).HasColumnName("is_on_hold_payment");
        
        builder.Property(e => e.LinkedJiraId).HasMaxLength(100).HasColumnName("linked_jira_id");
        
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");
        
        builder.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
        
        builder.Property(e => e.ModuleId).HasColumnName("module_id");
        
        builder.Property(e => e.PriorityScore).HasPrecision(4, 2).HasComment("Computed weighted priority score (0–5). Formula: (0.30×impact) + (0.20×urgency) + (0.15×sentiment) + (0.15×sla_severity) + (0.10×type) + (0.10×tier). Mapped: ≥4.5=P1(Urgent), 3.5–4.49=P2(High), 2.5–3.49=P3(Medium), <2.5=P4(Low).").HasColumnName("priority_score");
        
        builder.Property(e => e.PriorityScoreAt).HasComment("Timestamp of last priority score computation. Re-evaluation loop updates this on every customer reply or SLA status change.").HasColumnName("priority_score_at");
        
        builder.Property(e => e.ProductType).HasMaxLength(40).HasColumnName("product_type");
        
        builder.Property(e => e.Rca).HasColumnName("rca");
        
        builder.Property(e => e.SentimentScore).HasPrecision(4, 2).HasColumnName("sentiment_score");
        
        builder.Property(e => e.SlaExcluded).HasColumnName("sla_excluded");
        
        builder.Property(e => e.SlaSeverityScore).HasPrecision(4, 2).HasColumnName("sla_severity_score");
        
        builder.Property(e => e.SolutionType).HasMaxLength(40).HasColumnName("solution_type");
        
        builder.Property(e => e.SolutionTypeId).HasComment("FK to lookup.solution_types. Replaces the free-text solution_type varchar column. Both columns kept during migration; remove solution_type varchar after data backfill.").HasColumnName("solution_type_id");
        
        builder.Property(e => e.SubModuleId).HasColumnName("sub_module_id");
        
        builder.Property(e => e.Subject).HasColumnName("title");
        
        builder.Property(e => e.TicketNumber).HasMaxLength(20).HasColumnName("ticket_number");
        
        builder.Property(e => e.TierWeight).HasPrecision(4, 2).HasColumnName("tier_weight");
        
        builder.Property(e => e.TypeWeight).HasPrecision(4, 2).HasColumnName("type_weight");
        
        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        
        builder.Property(e => e.UrgencyScore).HasPrecision(4, 2).HasColumnName("urgency_score");
        
        builder.Property(e => e.VersionId).HasColumnName("version_id");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsRowVersion();

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AssignedAgentId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_assigned_agent_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AuditedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_audited_by_fkey");

        builder.HasOne<Adrenalin.Modules.Company.Domain.Entities.Company>().WithMany().HasForeignKey(d => d.CompanyId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("tickets_company_id_fkey");

        builder.HasOne<Contact>().WithMany().HasForeignKey(d => d.ContactId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_contact_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedByUserId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_created_by_user_id_fkey");

        builder.HasOne<TicketStatusGraph>().WithMany().HasForeignKey(d => d.GraphId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("tickets_graph_id_fkey");

        builder.HasOne<Group>().WithMany().HasForeignKey(d => d.GroupId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_group_id_fkey");

        builder.HasOne<Module>().WithMany().HasForeignKey(d => d.ModuleId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("tickets_module_id_fkey");

        builder.HasOne<SolutionType>().WithMany().HasForeignKey(d => d.SolutionTypeId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_solution_type_id_fkey");

        builder.HasOne<SubModule>().WithMany().HasForeignKey(d => d.SubModuleId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_sub_module_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_updated_by_fkey");

        builder.HasOne<ProductVersion>().WithMany().HasForeignKey(d => d.VersionId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("tickets_version_id_fkey");
    }
}
