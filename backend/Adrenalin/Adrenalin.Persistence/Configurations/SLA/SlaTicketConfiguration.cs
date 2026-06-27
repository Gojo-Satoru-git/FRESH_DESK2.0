// MERGED: main's existing SlaTicketConfiguration + 3 new column mappings
// (tenant_id, warned_at_80_pct, warned_at_30_min) from schema_sla_v11_postgres.sql.
// Everything else is unchanged from main — same indexes, same comments, same FKs.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class SlaTicketConfiguration : IEntityTypeConfiguration<SlaTicket>
{
    public void Configure(EntityTypeBuilder<SlaTicket> builder)
    {
        builder.HasKey(e => e.Id).HasName("sla_tickets_pkey");

        builder.ToTable("sla_tickets", "sla", tb => tb.HasComment("Per-ticket SLA clock. One-to-one with tickets (per tenant). paused_minutes accumulates total pause time from On Hold / Product Roadmap / Pending states. resolution_due_at is extended on resume. Breach flags set by SLA engine daemon / scheduled job."));

        builder.HasIndex(e => e.FollowUpDueAt, "idx_sla_follow_up_due").HasFilter("((follow_up_at IS NULL) AND (follow_up_due_at IS NOT NULL))");

        builder.HasIndex(e => new { e.FirstResponseBreached, e.ResolutionBreached }, "idx_sla_tickets_breached");

        builder.HasIndex(e => e.ResolutionDueAt, "idx_sla_tickets_due").HasFilter("(resolved_at IS NULL)");

        // ── v11: unique constraint is now (tenant_id, ticket_id), not ticket_id alone ──
        builder.HasIndex(e => new { e.TenantId, e.TicketId }, "uq_sla_tickets_tenant_ticket").IsUnique();

        // ── v11: new partial indexes for daemon warning scans ─────────────────
        builder.HasIndex(e => new { e.CreatedAt, e.ResolutionDueAt }, "idx_sla_tickets_warn_80pct")
            .HasFilter("resolved_at IS NULL AND resolution_breached = false AND warned_at_80_pct = false");

        builder.HasIndex(e => e.ResolutionDueAt, "idx_sla_tickets_warn_30min_res")
            .HasFilter("resolved_at IS NULL AND resolution_breached = false AND warned_at_30_min = false");

        builder.HasIndex(e => e.FirstResponseDueAt, "idx_sla_tickets_warn_30min_fr")
            .HasFilter("first_response_at IS NULL AND first_response_breached = false AND warned_at_30_min = false");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Property(e => e.FirstResponseAt).HasColumnName("first_response_at");

        builder.Ignore(e => e.RowVersion);

        builder.Property(e => e.FirstResponseBreached).HasColumnName("first_response_breached");

        builder.Property(e => e.FirstResponseDueAt).HasColumnName("first_response_due_at");

        builder.Property(e => e.FollowUpAt).HasColumnName("follow_up_at");

        builder.Property(e => e.FollowUpBreached).HasComment("TRUE when follow_up_at IS NULL AND NOW() > follow_up_due_at. Set by SLA engine daemon alongside first_response_breached and resolution_breached.").HasColumnName("follow_up_breached");

        builder.Property(e => e.FollowUpDueAt).HasComment("Third SLA stage: follow-up deadline. Typically set when ticket enters pending_customer status — agent must follow up if no customer response within N business hours. Prevents stale pending tickets.").HasColumnName("follow_up_due_at");

        builder.Property(e => e.LastPausedAt).HasColumnName("last_paused_at");

        builder.Property(e => e.PausedMinutes).HasColumnName("paused_minutes");

        builder.Property(e => e.PolicyId).HasColumnName("policy_id");

        builder.Property(e => e.ResolutionBreached).HasColumnName("resolution_breached");

        builder.Property(e => e.ResolutionDueAt).HasColumnName("resolution_due_at");

        builder.Property(e => e.ResolvedAt).HasColumnName("resolved_at");

        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        // ── NEW (v11) ──────────────────────────────────────────────────────────
        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .HasDefaultValue("default")
            .HasComment("Tenant that owns this SLA record (Gap 4). All daemon/handler queries filter on this.");

        builder.Property(e => e.WarnedAt80Pct)
            .HasColumnName("warned_at_80_pct")
            .HasDefaultValue(false)
            .HasComment("TRUE once the 80%-elapsed warning has been published. Prevents re-publishing every daemon tick.");

        builder.Property(e => e.WarnedAt30Min)
            .HasColumnName("warned_at_30_min")
            .HasDefaultValue(false)
            .HasComment("TRUE once the 30-min-before-breach warning has been published. Unified sentinel for both first-response and resolution windows.");

        builder.HasOne(d => d.Policy).WithMany(p => p.SlaTickets).HasForeignKey(d => d.PolicyId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("sla_tickets_policy_id_fkey");

        builder.HasOne<Ticket>().WithOne().HasForeignKey<SlaTicket>(d => d.TicketId).HasConstraintName("sla_tickets_ticket_id_fkey");
    }
}
