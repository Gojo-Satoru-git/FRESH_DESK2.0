using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.KnowledgeBase.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.KnowledgeBase;

public class KbFolderConfiguration : IEntityTypeConfiguration<KbFolder>
{
    public void Configure(EntityTypeBuilder<KbFolder> builder)
    {
        builder.HasKey(e => e.Id).HasName("kb_folders_pkey");

        builder.ToTable("kb_folders", "kb", tb => tb.HasComment("Self-referencing folder hierarchy. parent_id=NULL for root folders. Use WITH RECURSIVE CTE to retrieve full tree. Depth limit enforced in API layer."));

        builder.HasIndex(e => e.ParentId, "idx_kb_folders_parent").HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.DisplayOrder).HasColumnName("display_order");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");

        builder.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");

        builder.Property(e => e.ParentId).HasColumnName("parent_id");

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_folders_created_by_fkey");

        builder.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_folders_parent_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("kb_folders_updated_by_fkey");
    }
}
