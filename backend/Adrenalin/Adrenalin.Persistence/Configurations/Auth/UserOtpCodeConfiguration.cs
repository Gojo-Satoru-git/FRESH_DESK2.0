using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserOtpCodeConfiguration : IEntityTypeConfiguration<UserOtpCode>
{
    public void Configure(EntityTypeBuilder<UserOtpCode> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_otp_codes_pkey");

        builder.ToTable("user_otp_codes", "auth", tb => tb.HasComment("Hashed OTP codes for email/phone verification and 2FA. failed_attempts incremented on wrong guess; is_used=true on successful verification. Expired rows purged by nightly cleanup job."));

        builder.HasIndex(e => e.ExpiresAt, "idx_otp_expires").HasFilter("(is_used = false)");

        builder.HasIndex(e => new { e.UserId, e.Purpose }, "idx_otp_user_purpose").HasFilter("(is_used = false)");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CodeHash)
            .HasMaxLength(255)
            .HasColumnName("code_hash");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.DeliveryTarget)
            .HasMaxLength(255)
            .HasColumnName("delivery_target");

        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.FailedAttempts).HasColumnName("failed_attempts");
        builder.Property(e => e.IsUsed).HasColumnName("is_used");

        builder.Property(e => e.Purpose)
            .HasMaxLength(60)
            .HasColumnName("purpose");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");

        builder.HasOne(d => d.User).WithMany(p => p.UserOtpCodes)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_otp_codes_user_id_fkey");
    }
}
