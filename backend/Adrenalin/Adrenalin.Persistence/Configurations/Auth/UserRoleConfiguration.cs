using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_roles_pkey");

        builder.ToTable("user_roles", "auth");

        builder.HasIndex(e => e.RoleId, "idx_user_roles_role").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.UserId, "idx_user_roles_user").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.UserId, e.RoleId }, "uq_user_roles").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.AssignedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("assigned_at");

        builder.Property(e => e.AssignedBy).HasColumnName("assigned_by");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.RoleId).HasColumnName("role_id");
        builder.Ignore(e => e.RowVersion);
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_roles_assigned_by_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_roles_created_by_fkey");

        builder.HasOne(d => d.Role).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("user_roles_role_id_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_roles_updated_by_fkey");

        builder.HasOne(d => d.User).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_roles_user_id_fkey");
    }
}
