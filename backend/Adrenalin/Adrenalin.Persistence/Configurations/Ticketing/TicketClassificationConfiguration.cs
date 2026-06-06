using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketClassificationConfiguration : IEntityTypeConfiguration<TicketClassification>
{
    public void Configure(EntityTypeBuilder<TicketClassification> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_classification_pkey");

        builder.ToTable("ticket_classification", "ai", tb => tb.HasComment("One row per ticket — full C-R-L (Classifier-Retrieval-LLM) pipeline audit. classifier_auto_routed=TRUE means phase 2+3 were skipped (confidence ≥ threshold). retrieval_discrepancy=TRUE triggered Phase 3 LLM. llm_invoked tracks LLM cost exposure. model_version enables A/B testing."));

        builder.HasIndex(e => new { e.FinalLabel, e.ClassifiedAt }, "idx_ticket_classification_label").IsDescending(false, true);

        builder.HasIndex(e => new { e.LlmInvoked, e.ClassifiedAt }, "idx_ticket_classification_llm").IsDescending(false, true).HasFilter("(llm_invoked = true)");

        builder.HasIndex(e => e.TicketId, "idx_ticket_classification_ticket");

        builder.HasIndex(e => e.TicketId, "uq_ticket_classification_ticket").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.ClassifiedAt).HasDefaultValueSql("now()").HasColumnName("classified_at");
        
        builder.Property(e => e.ClassifierAutoRouted).HasColumnName("classifier_auto_routed");
        
        builder.Property(e => e.ClassifierConfidence).HasPrecision(5, 4).HasColumnName("classifier_confidence");
        
        builder.Property(e => e.ClassifierLabel).HasMaxLength(60).HasColumnName("classifier_label");
        
        builder.Property(e => e.FinalConfidence).HasPrecision(5, 4).HasColumnName("final_confidence");
        
        builder.Property(e => e.FinalLabel).HasMaxLength(60).HasColumnName("final_label");
        
        builder.Property(e => e.LlmFinalLabel).HasMaxLength(60).HasColumnName("llm_final_label");
        
        builder.Property(e => e.LlmInvoked).HasColumnName("llm_invoked");
        
        builder.Property(e => e.LlmReasoning).HasColumnName("llm_reasoning");
        
        builder.Property(e => e.ModelVersion).HasMaxLength(40).HasColumnName("model_version");
        
        builder.Property(e => e.RetrievalConsensusLabel).HasMaxLength(60).HasColumnName("retrieval_consensus_label");
        
        builder.Property(e => e.RetrievalDiscrepancy).HasColumnName("retrieval_discrepancy");
        
        builder.Property(e => e.RetrievalTopKLabels).HasColumnType("jsonb").HasColumnName("retrieval_top_k_labels");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.HasOne(d => d.Ticket).WithOne(p => p.TicketClassification).HasForeignKey<TicketClassification>(d => d.TicketId).HasConstraintName("ticket_classification_ticket_id_fkey");
    }
}
