
using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

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
       .HasColumnName("row_version")
       .IsRowVersion()
       .IsConcurrencyToken();

        // Assigned By

        

        // Created By

        

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