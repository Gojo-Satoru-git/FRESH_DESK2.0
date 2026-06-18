using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.AI.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.AI;

public class AiSuggestionLogConfiguration : IEntityTypeConfiguration<AiSuggestionLog>
{
    public void Configure(EntityTypeBuilder<AiSuggestionLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("ai_suggestion_logs_pkey");

        builder.ToTable("ai_suggestion_logs", "ai");

        builder.Ignore(e => e.RowVersion);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        builder.Property(e => e.RequestType).HasMaxLength(100).HasColumnName("request_type");
        builder.Property(e => e.PromptHash).HasMaxLength(64).HasColumnName("prompt_hash");
        builder.Property(e => e.ResponseHash).HasMaxLength(64).HasColumnName("response_hash");
        builder.Property(e => e.Provider).HasMaxLength(100).HasColumnName("provider");
        builder.Property(e => e.Model).HasMaxLength(100).HasColumnName("model");
        builder.Property(e => e.TokensUsed).HasColumnName("tokens_used");
        builder.Property(e => e.ExecutionTimeMs).HasColumnName("execution_time_ms");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.HasIndex(e => e.TicketId, "idx_ai_logs_ticket");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("ai_suggestion_logs_ticket_id_fkey");
    }
}
