using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class AgentStreakConfiguration : IEntityTypeConfiguration<AgentStreak>
{
    public void Configure(EntityTypeBuilder<AgentStreak> builder)
    {
        builder.HasKey(e => e.Id).HasName("agent_streaks_pkey");

        builder.ToTable("agent_streaks", "gamification", tb => tb.HasComment("One row per agent. current_streak_days = consecutive days with ≥1 SLA-met resolution. Nightly job resets if last_activity_date < CURRENT_DATE - 1 day."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => e.AgentId, "uq_agent_streaks_agent").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.CurrentStreakDays).HasColumnName("current_streak_days");
        
        builder.Property(e => e.LastActivityDate).HasColumnName("last_activity_date");
        
        builder.Property(e => e.LongestStreakDays).HasColumnName("longest_streak_days");
        
        builder.Property(e => e.StreakStartDate).HasColumnName("streak_start_date");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        builder.HasOne<User>().WithOne().HasForeignKey<AgentStreak>(d => d.AgentId).HasConstraintName("agent_streaks_agent_id_fkey");
    }
}
