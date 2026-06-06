using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.AI.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.AI;

public class AiSuggestionLogConfiguration : IEntityTypeConfiguration<AiSuggestionLog>
{
    public void Configure(EntityTypeBuilder<AiSuggestionLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("ai_suggestion_logs_pkey");

        builder.ToTable("ai_suggestion_logs", "ai", tb => tb.HasComment("AI suggestions shown to agent on new ticket. Two rows per ticket: troubleshooting_steps and similar_tickets. agent_rating feeds the ML training pipeline. ai_rating also stored redundantly on tickets.ai_rating for quick dashboard reporting."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.AgentId, e.CreatedAt }, "idx_ai_logs_agent").IsDescending(false, true);

        builder.HasIndex(e => e.TicketId, "idx_ai_logs_ticket");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.AgentId).HasColumnName("agent_id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            
        builder.Property(e => e.SuggestionContent).HasColumnName("suggestion_content");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AgentId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ai_suggestion_logs_agent_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("ai_suggestion_logs_ticket_id_fkey");
    }
}
