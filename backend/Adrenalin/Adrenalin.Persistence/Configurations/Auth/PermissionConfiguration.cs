using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.Id).HasName("permissions_pkey");

        builder.ToTable(
            "permissions", "auth", 
            tb => tb.HasComment(
                "Atomic resource:action pairs. Examples: ticket:assign, company:create, ticket:delete. Permissions are system-defined and should not change without a migration."));

        builder.HasIndex(e => new 
        { 
            e.Resource,
            e.Action 
        }, "uq_permissions_resource_action").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.Action)
            .HasMaxLength(60)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(e => e.Description)
            .HasColumnName("description");
        
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(e => e.Resource)
            .HasMaxLength(60)
            .HasColumnName("resource")
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasOne(d => d.CreatedByNavigation).WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("permissions_created_by_fkey");

        builder.HasOne(d => d.UpdatedByNavigation).WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("permissions_updated_by_fkey");
    }
}
