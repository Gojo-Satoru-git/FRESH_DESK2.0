using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class AgentBadgeConfiguration : IEntityTypeConfiguration<AgentBadge>
{
    public void Configure(EntityTypeBuilder<AgentBadge> builder)
    {
        builder.HasKey(e => e.Id).HasName("agent_badges_pkey");

        builder.ToTable("agent_badges", "gamification");

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.AgentId, e.EarnedAt }, "idx_agent_badges_agent").IsDescending(false, true);

        builder.HasIndex(e => e.Notified, "idx_agent_badges_unnotified").HasFilter("(notified = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.BadgeId).HasColumnName("badge_id");
        
        builder.Property(e => e.EarnedAt).HasDefaultValueSql("now()").HasColumnName("earned_at");
            
        builder.Property(e => e.Notified).HasColumnName("notified");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AgentId).HasConstraintName("agent_badges_agent_id_fkey");

        builder.HasOne(d => d.Badge).WithMany(p => p.AgentBadges).HasForeignKey(d => d.BadgeId).HasConstraintName("agent_badges_badge_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("agent_badges_ticket_id_fkey");
    }
}
