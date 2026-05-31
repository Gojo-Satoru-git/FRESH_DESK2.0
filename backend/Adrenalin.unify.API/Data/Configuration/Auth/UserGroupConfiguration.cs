using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class UserGroupConfiguration
    : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("user_groups", "auth");

        builder.HasKey(x => x.Id)
               .HasName("user_groups_pkey");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_user_groups_user")
               .HasFilter("(is_deleted = false)");

        builder.HasIndex(x => x.GroupId)
               .HasDatabaseName("idx_user_groups_group")
               .HasFilter("(is_deleted = false)");

        builder.HasIndex(x => new
        {
            x.UserId,
            x.GroupId
        })
        .IsUnique()
        .HasDatabaseName("uq_user_groups");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.GroupId)
               .HasColumnName("group_id");

        builder.Property(x => x.IsLead)
               .HasColumnName("is_lead");

        builder.Property(x => x.IsDeleted)
               .HasColumnName("is_deleted");

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.RowVersion)
               .HasColumnName("row_version");

        // Created By

        builder.HasOne(x => x.CreatedByNavigation)
               .WithMany(x => x.UserGroupCreatedByNavigations)
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("user_groups_created_by_fkey");

        // User

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserGroups)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("user_groups_user_id_fkey");

        // Group

        builder.HasOne(x => x.Group)
               .WithMany(x => x.UserGroups)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("user_groups_group_id_fkey");
    }
}