using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class LeaderboardSnapshotConfiguration : IEntityTypeConfiguration<LeaderboardSnapshot>
{
    public void Configure(EntityTypeBuilder<LeaderboardSnapshot> builder)
    {
        builder.HasKey(e => e.Id).HasName("leaderboard_snapshots_pkey");

        builder.ToTable("leaderboard_snapshots", "gamification", tb => tb.HasComment("Pre-computed daily/weekly/monthly leaderboards. Rebuilt at 00:05 by scheduled job. Reading live SUM on agent_points at request time is too expensive at scale."));

        builder.Ignore(e => e.RowVersion);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.AvgCsat).HasPrecision(4, 2).HasColumnName("avg_csat");
            
        builder.Property(e => e.ComputedAt).HasDefaultValueSql("now()").HasColumnName("computed_at");
            
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        
        builder.Property(e => e.PeriodEnd).HasColumnName("period_end");
        
        builder.Property(e => e.PeriodStart).HasColumnName("period_start");
        
        builder.Property(e => e.Rank).HasColumnName("rank");
        
        builder.Property(e => e.SlaMetCount).HasColumnName("sla_met_count");
        
        builder.Property(e => e.TicketsResolved).HasColumnName("tickets_resolved");
        
        builder.Property(e => e.TotalPoints).HasColumnName("total_points");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AgentId).HasConstraintName("leaderboard_snapshots_agent_id_fkey");

        builder.HasOne<Group>().WithMany().HasForeignKey(d => d.GroupId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("leaderboard_snapshots_group_id_fkey");
    }
}
