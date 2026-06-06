using Adrenalin.Modules.KB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public sealed class KbFolderConfiguration : IEntityTypeConfiguration<KbFolder>
{
    public void Configure(EntityTypeBuilder<KbFolder> builder)
    {
        builder.ToTable("kb_folders", "kb");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        // ── Concurrency token — NOT present in schema, ignore ─────────────────
        // kb.kb_folders has no row_version column.
        builder.Ignore(f => f.RowVersion);

        // ── Columns ───────────────────────────────────────────────────────────
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name");

        builder.Property(f => f.ParentId)
            .HasColumnName("parent_id");

        builder.Property(f => f.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(f => f.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(f => f.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(f => f.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // ── Self-referencing relationship ─────────────────────────────────────
        builder.HasOne(f => f.Parent)
            .WithMany()
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(f => f.ParentId)
            .HasDatabaseName("ix_kb_folders_parent_id");

        builder.HasIndex(f => new { f.ParentId, f.DisplayOrder })
            .HasDatabaseName("ix_kb_folders_parent_display_order");

        // ── Global query filter — hide soft-deleted rows by default ───────────
        builder.HasQueryFilter(f => !f.IsDeleted);

        // ── Ignore domain events (not persisted) ──────────────────────────────
        builder.Ignore(f => f.DomainEvents);
    }
}
