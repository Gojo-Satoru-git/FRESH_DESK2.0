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

        builder.ToTable("sla_tickets", "sla", tb => tb.HasComment("Per-ticket SLA clock. One-to-one with tickets. paused_minutes accumulates total pause time from On Hold / Product Roadmap / Pending states. resolution_due_at is extended on resume. Breach flags set by SLA engine daemon / scheduled job."));

        builder.HasIndex(e => e.FollowUpDueAt, "idx_sla_follow_up_due").HasFilter("((follow_up_at IS NULL) AND (follow_up_due_at IS NOT NULL))");

        builder.HasIndex(e => new { e.FirstResponseBreached, e.ResolutionBreached }, "idx_sla_tickets_breached");

        builder.HasIndex(e => e.ResolutionDueAt, "idx_sla_tickets_due").HasFilter("(resolved_at IS NULL)");

        builder.HasIndex(e => e.TicketId, "uq_sla_tickets_ticket").IsUnique();

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

        builder.HasOne(d => d.Policy).WithMany(p => p.SlaTickets).HasForeignKey(d => d.PolicyId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("sla_tickets_policy_id_fkey");

        builder.HasOne<Ticket>().WithOne().HasForeignKey<SlaTicket>(d => d.TicketId).HasConstraintName("sla_tickets_ticket_id_fkey");
    }
}
