using Adrenalin.Modules.KB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

// ─── KbAttachment ─────────────────────────────────────────────────────────────

public sealed class KbAttachmentConfiguration : IEntityTypeConfiguration<KbAttachment>
{
    public void Configure(EntityTypeBuilder<KbAttachment> builder)
    {
        builder.ToTable("kb_attachments", "kb");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id");

        // Schema: id, article_id, file_url, file_name, file_size_bytes, mime_type,
        //         is_deleted, created_at
        // No: row_version, updated_at, updated_by, created_by, is_active
        builder.Ignore(a => a.RowVersion);

        builder.Property(a => a.ArticleId)
            .IsRequired()
            .HasColumnName("article_id");

        builder.Property(a => a.FileUrl)
            .IsRequired()
            .HasColumnName("file_url");

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("file_name");

        builder.Property(a => a.FileSizeBytes)
            .HasColumnName("file_size_bytes");

        builder.Property(a => a.MimeType)
            .HasMaxLength(100)
            .HasColumnName("mime_type");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.HasIndex(a => a.ArticleId)
            .HasDatabaseName("ix_kb_attachments_article_id");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

// ─── PortalBanner ─────────────────────────────────────────────────────────────

public sealed class PortalBannerConfiguration : IEntityTypeConfiguration<PortalBanner>
{
    public void Configure(EntityTypeBuilder<PortalBanner> builder)
    {
        builder.ToTable("portal_banners", "kb");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");

        builder.Ignore(b => b.RowVersion);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(b => b.Message)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("message");

        builder.Property(b => b.ActiveFrom)
            .HasColumnName("active_from");

        builder.Property(b => b.ActiveTo)
            .HasColumnName("active_to");

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedBy).HasColumnName("created_by");
        builder.Property(b => b.UpdatedBy).HasColumnName("updated_by");

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.HasIndex(b => b.IsActive)
            .HasDatabaseName("ix_portal_banners_is_active");
    }
}
