using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_groups_pkey");

        builder.ToTable("user_groups", "auth");

        builder.HasIndex(e => e.GroupId, "idx_user_groups_group").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.GroupId, e.IsLead }, "idx_user_groups_lead").HasFilter("((is_lead = true) AND (is_deleted = false))");

        builder.HasIndex(e => e.UserId, "idx_user_groups_user").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.UserId, e.GroupId }, "uq_user_groups").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.GroupId).HasColumnName("group_id");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.IsLead).HasColumnName("is_lead");

        builder.Ignore(e => e.RowVersion);
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_groups_updated_by_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_groups_created_by_fkey");

        builder.HasOne(d => d.Group).WithMany(p => p.UserGroups)
            .HasForeignKey(d => d.GroupId)
            .HasConstraintName("user_groups_group_id_fkey");

        builder.HasOne(d => d.User).WithMany(p => p.UserGroups)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_groups_user_id_fkey");
    }
}
