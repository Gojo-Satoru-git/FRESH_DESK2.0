using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.KnowledgeBase.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public class KbArticleConfiguration : IEntityTypeConfiguration<KbArticle>
{
    public void Configure(EntityTypeBuilder<KbArticle> builder)
    {
        builder.HasKey(e => e.Id).HasName("kb_articles_pkey");

        builder.ToTable("kb_articles", "kb");

        builder.HasIndex(e => new { e.AutoResolve, e.GuardrailExcluded }, "idx_kb_articles_auto_resolve").HasFilter("((auto_resolve = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.FolderId, "idx_kb_articles_folder").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Keywords, "idx_kb_articles_keywords").HasFilter("((auto_resolve = true) AND (is_deleted = false))").HasMethod("gin");

        builder.HasIndex(e => new { e.IsPublished, e.ArticleType }, "idx_kb_articles_published").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.Title, "idx_kb_articles_title_trgm").HasFilter("(is_deleted = false)").HasMethod("gin").HasOperators(new[] { "gin_trgm_ops" });

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.ArticleType).HasMaxLength(40).HasColumnName("article_type");

        builder.Property(e => e.AuthorId).HasColumnName("author_id");

        builder.Property(e => e.AutoResolve).HasComment("TRUE = this article is eligible for the auto-resolution engine. Engine only fires if confidence > confidence_threshold AND guardrail_excluded = FALSE.").HasColumnName("auto_resolve");

        builder.Property(e => e.ConfidenceThreshold).HasPrecision(4, 3).HasDefaultValue(0.850m).HasComment("Minimum match confidence (0.85 default) required to trigger auto-resolve. Articles with high reopen rates should have this raised automatically by learning loop.").HasColumnName("confidence_threshold");

        builder.Property(e => e.Content).HasColumnName("content");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.FolderId).HasColumnName("folder_id");

        builder.Property(e => e.GuardrailExcluded).HasComment("TRUE = this article covers a guardrail topic (payroll, financial, legal/compliance). Auto-resolution engine NEVER fires for guardrail_excluded articles regardless of confidence.").HasColumnName("guardrail_excluded");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.IsPublished).HasColumnName("is_published");

        builder.Property(e => e.Keywords).HasComment("Phase 1 (keyword match) trigger words. Stored as PostgreSQL text array. Example: ARRAY['forgot password', 'reset password', 'login failed'].").HasColumnName("keywords");

        builder.Property(e => e.ResolutionText).HasColumnName("resolution_text");

        builder.Property(e => e.Status).HasMaxLength(30).HasDefaultValueSql("'draft'::character varying").HasColumnName("status");

        builder.Property(e => e.TimesMatched).HasComment("Learning loop counter: incremented each time this article is matched (auto-resolve attempted).").HasColumnName("times_matched");

        builder.Property(e => e.TimesReopened).HasComment("Learning loop counter: incremented each time a ticket auto-resolved via this article is reopened. High reopen rate → confidence_threshold auto-raised by learning loop job.").HasColumnName("times_reopened");

        builder.Property(e => e.Title).HasMaxLength(300).HasColumnName("title");

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AuthorId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_articles_author_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_articles_created_by_fkey");

        builder.HasOne(d => d.Folder).WithMany(p => p.KbArticles).HasForeignKey(d => d.FolderId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_articles_folder_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_articles_updated_by_fkey");
    }
}
