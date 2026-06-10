using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => e.Id).HasName("role_permissions_pkey");

        builder.ToTable("role_permissions", "auth");

        builder.HasIndex(e => e.RoleId, "idx_role_permissions_role");

        builder.HasIndex(e => new { e.RoleId, e.PermissionId }, "uq_role_permissions").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.PermissionId).HasColumnName("permission_id");
        builder.Property(e => e.RoleId).HasColumnName("role_id");
        builder.Ignore(e => e.RowVersion);
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne(d => d.CreatedByNavigation).WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("role_permissions_created_by_fkey");

        builder.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.PermissionId)
            .HasConstraintName("role_permissions_permission_id_fkey");

        builder.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("role_permissions_role_id_fkey");

        builder.HasOne(d => d.UpdatedByNavigation).WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("role_permissions_updated_by_fkey");
    }
}
