
using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

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
       .HasColumnName("row_version")
       .IsRowVersion()
       .IsConcurrencyToken();
       builder.HasQueryFilter(x => !x.IsDeleted);

        // Audit Relationships

        builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.CreatedBy)
       .OnDelete(DeleteBehavior.SetNull);

builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.UpdatedBy)
       .OnDelete(DeleteBehavior.SetNull);

        // RolePermission Relationship

        builder.HasMany(x => x.RolePermissions)
               .WithOne(x => x.Permission)
               .HasForeignKey(x => x.PermissionId);
    }
}

