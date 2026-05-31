using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable(
            "permissions",
            "auth",
            tb => tb.HasComment(
                "Atomic resource:action pairs. Examples: ticket:assign, company:create, ticket:delete. Permissions are system-defined and should not change without a migration."));

        builder.HasKey(x => x.Id)
               .HasName("permissions_pkey");

        builder.HasIndex(x => new
        {
            x.Resource,
            x.Action
        })
        .IsUnique()
        .HasDatabaseName("uq_permissions_resource_action");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Resource)
               .HasColumnName("resource")
               .HasMaxLength(60)
               .IsRequired();

        builder.Property(x => x.Action)
               .HasColumnName("action")
               .HasMaxLength(60)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasColumnName("description");

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

        // Audit Relationships

        builder.HasOne(x => x.CreatedByNavigation)
               .WithMany(x => x.PermissionCreatedByNavigations)
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("permissions_created_by_fkey");

        builder.HasOne(x => x.UpdatedByNavigation)
               .WithMany(x => x.PermissionUpdatedByNavigations)
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("permissions_updated_by_fkey");

        // RolePermission Relationship

        builder.HasMany(x => x.RolePermissions)
               .WithOne(x => x.Permission)
               .HasForeignKey(x => x.PermissionId);
    }
}

