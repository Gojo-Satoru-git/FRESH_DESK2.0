using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "auth");

        builder.HasKey(x => x.Id)
               .HasName("user_roles_pkey");

        builder.HasIndex(x => x.RoleId)
               .HasDatabaseName("idx_user_roles_role")
               .HasFilter("(is_deleted = false)");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_user_roles_user")
               .HasFilter("(is_deleted = false)");

        builder.HasIndex(x => new
        {
            x.UserId,
            x.RoleId
        })
        .IsUnique()
        .HasDatabaseName("uq_user_roles");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.RoleId)
               .HasColumnName("role_id");

        builder.Property(x => x.AssignedAt)
               .HasColumnName("assigned_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.AssignedBy)
               .HasColumnName("assigned_by");

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

        // Assigned By

        builder.HasOne(x => x.AssignedByNavigation)
               .WithMany(x => x.UserRoleAssignedByNavigations)
               .HasForeignKey(x => x.AssignedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("user_roles_assigned_by_fkey");

        // Created By

        builder.HasOne(x => x.CreatedByNavigation)
               .WithMany(x => x.UserRoleCreatedByNavigations)
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("user_roles_created_by_fkey");

        // Updated By

        builder.HasOne(x => x.UpdatedByNavigation)
               .WithMany(x => x.UserRoleUpdatedByNavigations)
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("user_roles_updated_by_fkey");

        // User

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserRoles)
               .HasForeignKey(x => x.UserId)
               .HasConstraintName("user_roles_user_id_fkey");

        // Role

        builder.HasOne(x => x.Role)
               .WithMany(x => x.UserRoles)
               .HasForeignKey(x => x.RoleId)
               .HasConstraintName("user_roles_role_id_fkey");
    }
}