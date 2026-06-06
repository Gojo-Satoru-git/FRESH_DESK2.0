using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Gamification.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Gamification;

public class AgentChallengeConfiguration : IEntityTypeConfiguration<AgentChallenge>
{
    public void Configure(EntityTypeBuilder<AgentChallenge> builder)
    {
        builder.HasKey(e => e.Id).HasName("agent_challenges_pkey");

        builder.ToTable("agent_challenges", "gamification");

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.AgentId, e.IsCompleted }, "idx_agent_challenges_agent");

        builder.HasIndex(e => new { e.ChallengeId, e.IsCompleted }, "idx_agent_challenges_challenge");

        builder.HasIndex(e => new { e.AgentId, e.ChallengeId }, "uq_agent_challenges").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.ChallengeId).HasColumnName("challenge_id");
        
        builder.Property(e => e.CompletedAt).HasColumnName("completed_at");
        
        builder.Property(e => e.CurrentValue).HasColumnName("current_value");
        
        builder.Property(e => e.IsCompleted).HasColumnName("is_completed");
        
        builder.Property(e => e.PointsAwarded).HasColumnName("points_awarded");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AgentId).HasConstraintName("agent_challenges_agent_id_fkey");

        builder.HasOne(d => d.Challenge).WithMany(p => p.AgentChallenges).HasForeignKey(d => d.ChallengeId).HasConstraintName("agent_challenges_challenge_id_fkey");
    }
}
