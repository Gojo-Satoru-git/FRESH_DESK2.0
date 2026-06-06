using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public sealed class KbArticleConfiguration : IEntityTypeConfiguration<KbArticle>
{
    public void Configure(EntityTypeBuilder<KbArticle> builder)
    {
        builder.ToTable("kb_articles", "kb");

        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Ignore(a => a.RowVersion);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        builder.Property(a => a.Content)
            .HasColumnName("content")
            .HasColumnType("text");

        builder.Property(a => a.ArticleType)
            .IsRequired()
            .HasColumnName("article_type")
            .HasConversion(
                v => v.ToString().ToSnakeCase(),
                v => Enum.Parse<ArticleType>(v, ignoreCase: true));

        builder.Property(a => a.Status)
            .IsRequired()
            .HasColumnName("status")
            .HasDefaultValue(ArticleStatus.Draft)
            .HasConversion(
                v => v.ToString().ToLower(),
                v => Enum.Parse<ArticleStatus>(v, ignoreCase: true));

        builder.Property(a => a.IsPublished)
            .HasColumnName("is_published")
            .HasDefaultValue(false);

        builder.Property(a => a.AuthorId)
            .HasColumnName("author_id");

        builder.Property(a => a.FolderId)
            .HasColumnName("folder_id");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(a => a.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // ── Auto-resolve columns (addendum v8) ────────────────────────────────
        builder.Property(a => a.AutoResolve)
            .HasColumnName("auto_resolve")
            .HasDefaultValue(false);

        // Schema: numeric(4,3) — matches ConfidenceThreshold range 0.500–1.000
        builder.Property(a => a.ConfidenceThresholdValue)
            .HasColumnName("confidence_threshold")
            .HasColumnType("numeric(4,3)")
            .HasDefaultValue(0.850m);

        // keywords stored as TEXT[] in Postgres
        builder.Property(a => a.Keywords)
            .HasColumnName("keywords")
            .HasColumnType("text[]");

        builder.Property(a => a.ResolutionText)
            .HasColumnName("resolution_text")
            .HasColumnType("text");

        builder.Property(a => a.GuardrailExcluded)
            .HasColumnName("guardrail_excluded")
            .HasDefaultValue(false);

        builder.Property(a => a.TimesMatched)
            .HasColumnName("times_matched")
            .HasDefaultValue(0);

        builder.Property(a => a.TimesReopened)
            .HasColumnName("times_reopened")
            .HasDefaultValue(0);

        // ── Computed/ignored ──────────────────────────────────────────────────
        builder.Ignore(a => a.ConfidenceThreshold);
        builder.Ignore(a => a.DomainEvents);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(a => a.Folder)
            .WithMany()
            .HasForeignKey(a => a.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Attachments)
            .WithOne()
            .HasForeignKey(att => att.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("ix_kb_articles_status");

        builder.HasIndex(a => a.FolderId)
            .HasDatabaseName("ix_kb_articles_folder_id");

        builder.HasIndex(a => new { a.AutoResolve, a.GuardrailExcluded })
            .HasDatabaseName("ix_kb_articles_auto_resolve");

        // ── Global query filter ───────────────────────────────────────────────
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

/// <summary>
/// Simple extension to convert PascalCase enum names to snake_case for DB storage.
/// e.g. ReleaseNote → release_note
/// </summary>
internal static class StringExtensions
{
    internal static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return System.Text.RegularExpressions.Regex
            .Replace(value, "(?<!^)([A-Z])", "_$1")
            .ToLower();
    }
}
