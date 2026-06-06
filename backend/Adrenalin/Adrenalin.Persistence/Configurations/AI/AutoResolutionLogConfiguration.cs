using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.AI.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.AI;

public class AutoResolutionLogConfiguration : IEntityTypeConfiguration<AutoResolutionLog>
{
    public void Configure(EntityTypeBuilder<AutoResolutionLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("auto_resolution_log_pkey");

        builder.ToTable("auto_resolution_log", "ai", tb => tb.HasComment("Full audit of auto-resolution engine activity. One row per resolution attempt (successful or blocked). was_reopened=TRUE is the primary negative signal for the learning loop — the learning job reads this to raise confidence_threshold on kb_articles with high reopen rates. blocked_guardrail action means payroll/financial/legal guardrail fired."));

        builder.Ignore(e => e.RowVersion);

        builder.HasIndex(e => new { e.ActionTaken, e.MatchedAt }, "idx_auto_res_action").IsDescending(false, true);

        builder.HasIndex(e => new { e.KbArticleId, e.MatchedAt }, "idx_auto_res_article").IsDescending(false, true);

        builder.HasIndex(e => new { e.WasReopened, e.MatchedAt }, "idx_auto_res_reopened").IsDescending(false, true).HasFilter("(was_reopened = true)");

        builder.HasIndex(e => new { e.TicketId, e.MatchedAt }, "idx_auto_res_ticket").IsDescending(false, true);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.ActionTaken).HasMaxLength(30).HasColumnName("action_taken");
            
        builder.Property(e => e.BlockedReason).HasColumnName("blocked_reason");
        
        builder.Property(e => e.ConfidenceThreshold).HasPrecision(5, 4).HasColumnName("confidence_threshold");
            
        builder.Property(e => e.FinalConfidence).HasPrecision(5, 4).HasColumnName("final_confidence");
            
        builder.Property(e => e.KbArticleId).HasColumnName("kb_article_id");
        
        builder.Property(e => e.KeywordMatches).HasColumnName("keyword_matches").HasField("_keywordMatches");
        
        builder.Property(e => e.MatchPhase).HasMaxLength(20).HasColumnName("match_phase");
            
        builder.Property(e => e.MatchedAt).HasDefaultValueSql("now()").HasColumnName("matched_at");
            
        builder.Property(e => e.ReopenReason).HasColumnName("reopen_reason");
        
        builder.Property(e => e.ReopenedAt).HasColumnName("reopened_at");
        
        builder.Property(e => e.ResolutionChannel).HasMaxLength(20).HasColumnName("resolution_channel");
            
        builder.Property(e => e.ResolutionSentAt).HasColumnName("resolution_sent_at");
        
        builder.Property(e => e.SemanticSimilarity).HasPrecision(5, 4).HasColumnName("semantic_similarity");
            
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.WasReopened).HasColumnName("was_reopened");

        builder.HasOne<KbArticle>().WithMany().HasForeignKey(d => d.KbArticleId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("auto_resolution_log_kb_article_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("auto_resolution_log_ticket_id_fkey");
    }
}
