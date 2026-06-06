using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class AgentPointConfiguration : IEntityTypeConfiguration<AgentPoint>
{
    public void Configure(EntityTypeBuilder<AgentPoint> builder)
    {
        builder.HasKey(e => e.Id).HasName("agent_points_pkey");

        builder.ToTable("agent_points", "gamification", tb => tb.HasComment("Immutable append-only point ledger. Event-sourcing pattern. Total = SUM(points). Enables full audit, point reversals (insert negative row), time-travel queries. Never update or delete — insert only."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.AgentId, e.EarnedAt }, "idx_agent_points_agent").IsDescending(false, true);

        builder.HasIndex(e => new { e.AgentId, e.RuleId, e.EarnedDate }, "idx_agent_points_rule_daily");

        builder.HasIndex(e => e.TicketId, "idx_agent_points_ticket").HasFilter("(ticket_id IS NOT NULL)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.Description).HasMaxLength(200).HasColumnName("description");
            
        builder.Property(e => e.EarnedAt).HasDefaultValueSql("now()").HasColumnName("earned_at");
            
        builder.Property(e => e.EarnedDate).HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text))::date").HasColumnName("earned_date");
            
        builder.Property(e => e.Points).HasColumnName("points");
        
        builder.Property(e => e.RuleId).HasColumnName("rule_id");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AgentId).HasConstraintName("agent_points_agent_id_fkey");

        builder.HasOne(d => d.Rule).WithMany().HasForeignKey(d => d.RuleId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("agent_points_rule_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("agent_points_ticket_id_fkey");
    }
}
