using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "auth");

        builder.HasKey(x => x.Id)
               .HasName("role_permissions_pkey");

        builder.HasIndex(x => x.RoleId)
               .HasDatabaseName("idx_role_permissions_role");

        builder.HasIndex(x => new
        {
            x.RoleId,
            x.PermissionId
        })
        .IsUnique()
        .HasDatabaseName("uq_role_permissions");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.RoleId)
               .HasColumnName("role_id");

        builder.Property(x => x.PermissionId)
               .HasColumnName("permission_id");

        builder.Property(x => x.IsDeleted)
               .HasColumnName("is_deleted");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at");

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
               .HasColumnName("updated_by");

        builder.Property(x => x.RowVersion)
               .HasColumnName("row_version");

        // Created By

        builder.HasOne(x => x.CreatedByNavigation)
               .WithMany(x => x.RolePermissionCreatedByNavigations)
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("role_permissions_created_by_fkey");

        // Updated By

        builder.HasOne(x => x.UpdatedByNavigation)
               .WithMany(x => x.RolePermissionUpdatedByNavigations)
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("role_permissions_updated_by_fkey");

        // Permission

        builder.HasOne(x => x.Permission)
               .WithMany(x => x.RolePermissions)
               .HasForeignKey(x => x.PermissionId)
               .HasConstraintName("role_permissions_permission_id_fkey");

        // Role

        builder.HasOne(x => x.Role)
               .WithMany(x => x.RolePermissions)
               .HasForeignKey(x => x.RoleId)
               .HasConstraintName("role_permissions_role_id_fkey");
    }
}