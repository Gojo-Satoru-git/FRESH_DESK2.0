using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketRiskScoreConfiguration : IEntityTypeConfiguration<TicketRiskScore>
{
    public void Configure(EntityTypeBuilder<TicketRiskScore> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_risk_scores_pkey");

        builder.ToTable("ticket_risk_scores", "ai", tb => tb.HasComment("Full audit of every priority score computation run. Multiple rows per ticket because the re-evaluation loop fires on every customer reply and SLA status change. trigger_event shows what caused the re-score. customer_reply_count_at enables auditing how many replies pushed priority up."));

        builder.HasIndex(e => new { e.ForceP1Triggered, e.ComputedAt }, "idx_risk_scores_force_p1").IsDescending(false, true).HasFilter("(force_p1_triggered = true)");

        builder.HasIndex(e => new { e.FinalScore, e.ComputedAt }, "idx_risk_scores_high_priority").IsDescending().HasFilter("(final_score >= 3.5)");

        builder.HasIndex(e => new { e.TicketId, e.ComputedAt }, "idx_risk_scores_ticket").IsDescending(false, true);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AssignedPriority).HasMaxLength(20).HasColumnName("assigned_priority");
        
        builder.Property(e => e.BusinessCriticalBump).HasColumnName("business_critical_bump");
        
        builder.Property(e => e.CompetitorThreatFlag).HasColumnName("competitor_threat_flag");
        
        builder.Property(e => e.ComputedAt).HasDefaultValueSql("now()").HasColumnName("computed_at");
        
        builder.Property(e => e.CustomerReplyCountAt).HasColumnName("customer_reply_count_at");
        
        builder.Property(e => e.FinalScore).HasPrecision(4, 2).HasColumnName("final_score");
        
        builder.Property(e => e.ForceP1Reason).HasMaxLength(100).HasColumnName("force_p1_reason");
        
        builder.Property(e => e.ForceP1Triggered).HasColumnName("force_p1_triggered");
        
        builder.Property(e => e.ImpactScore).HasPrecision(4, 2).HasColumnName("impact_score");
        
        builder.Property(e => e.ModulesAffectedScore).HasPrecision(4, 2).HasColumnName("modules_affected_score");
        
        builder.Property(e => e.PilotProjectFlag).HasColumnName("pilot_project_flag");
        
        builder.Property(e => e.SentimentLabel).HasMaxLength(30).HasColumnName("sentiment_label");
        
        builder.Property(e => e.SentimentScore).HasPrecision(4, 2).HasColumnName("sentiment_score");
        
        builder.Property(e => e.SlaSeverityScore).HasPrecision(4, 2).HasColumnName("sla_severity_score");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.TierWeight).HasPrecision(4, 2).HasColumnName("tier_weight");
        
        builder.Property(e => e.TriggerEvent).HasMaxLength(60).HasColumnName("trigger_event");
        
        builder.Property(e => e.TypeWeight).HasPrecision(4, 2).HasColumnName("type_weight");
        
        builder.Property(e => e.UrgencyKeywords).HasColumnName("urgency_keywords");
        
        builder.Property(e => e.UrgencyScore).HasPrecision(4, 2).HasColumnName("urgency_score");
        
        builder.Property(e => e.UsersAffectedScore).HasPrecision(4, 2).HasColumnName("users_affected_score");
        
        builder.Ignore(e => e.RowVersion);

        builder.HasOne(d => d.Ticket).WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("ticket_risk_scores_ticket_id_fkey");
    }
}
