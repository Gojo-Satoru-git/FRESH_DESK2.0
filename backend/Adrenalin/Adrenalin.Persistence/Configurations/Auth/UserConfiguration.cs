using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id).HasName("users_pkey");

        builder.ToTable("users", "auth", tb => tb.HasComment("Core identity for all internal staff. Email uniqueness enforced case-insensitively. password_hash is bcrypt — never store plain. Soft-delete via is_deleted."));

        builder.HasIndex(e => e.IsActive, "idx_users_active").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.LastLoginAt, "idx_users_last_login")
            .IsDescending()
            .HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.LockoutEnd, "idx_users_lockout").HasFilter("((lockout_end IS NOT NULL) AND (is_deleted = false))");

        builder.HasIndex(e => e.NormalizedUsername, "idx_users_username").HasFilter("((is_deleted = false) AND (normalized_username IS NOT NULL))");

        builder.HasIndex(e => e.NormalizedEmail, "uq_users_normalized_email")
            .IsUnique()
            .HasFilter("(is_deleted = false)");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.AvatarUrl).HasColumnName("avatar_url");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.EmailVerified).HasColumnName("email_verified");
        builder.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");
        builder.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.LastLoginAt).HasColumnName("last_login_at");

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(e => e.LockoutEnd).HasColumnName("lockout_end");

        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(255)
            .HasColumnName("normalized_email");

        builder.Property(e => e.NormalizedUsername)
            .HasMaxLength(100)
            .HasColumnName("normalized_username");

        builder.Property(e => e.PasswordChangedAt).HasColumnName("password_changed_at");

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .HasColumnName("password_hash");

        builder.Property(e => e.Phone)
            .HasMaxLength(30)
            .HasColumnName("phone");

        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.Property(e => e.Username)
            .HasMaxLength(100)
            .HasColumnName("username");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("users_created_by_fkey");

        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("users_updated_by_fkey");
    }
}
