using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.KnowledgeBase.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public class KbAttachmentConfiguration : IEntityTypeConfiguration<KbAttachment>
{
    public void Configure(EntityTypeBuilder<KbAttachment> builder)
    {
        builder.HasKey(e => e.Id).HasName("kb_attachments_pkey");

        builder.ToTable("kb_attachments", "kb");

        builder.HasIndex(e => e.ArticleId, "idx_kb_attachments_article").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.ArticleId).HasColumnName("article_id");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Property(e => e.FileName).HasMaxLength(255).HasColumnName("file_name");

        builder.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");

        builder.Property(e => e.FileUrl).HasColumnName("file_url");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.MimeType).HasMaxLength(100).HasColumnName("mime_type");

        builder.HasOne(d => d.Article).WithMany(p => p.KbAttachments).HasForeignKey(d => d.ArticleId).HasConstraintName("kb_attachments_article_id_fkey");
    }
}
