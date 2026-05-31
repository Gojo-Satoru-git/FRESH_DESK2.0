using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class UserOtpCodeConfiguration
    : IEntityTypeConfiguration<UserOtpCode>
{
    public void Configure(EntityTypeBuilder<UserOtpCode> builder)
    {
        builder.ToTable(
            "user_otp_codes",
            "auth",
            tb => tb.HasComment(
                "Hashed OTP codes for email/phone verification and 2FA. FailedAttempts incremented on wrong guess; IsUsed=true on successful verification."));

        builder.HasKey(x => x.Id)
               .HasName("user_otp_codes_pkey");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_user_otp_codes_user");

        builder.HasIndex(x => x.CodeHash)
               .HasDatabaseName("idx_user_otp_codes_code_hash");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.CodeHash)
               .HasColumnName("code_hash")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Purpose)
               .HasColumnName("purpose")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.DeliveryTarget)
               .HasColumnName("delivery_target")
               .HasMaxLength(255);

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.VerifiedAt)
               .HasColumnName("verified_at");

        builder.Property(x => x.FailedAttempts)
               .HasColumnName("failed_attempts")
               .HasDefaultValue(0);

        builder.Property(x => x.IsUsed)
               .HasColumnName("is_used")
               .HasDefaultValue(false);

        // User Relationship

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserOtpCodes)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName(
                    "user_otp_codes_user_id_fkey");
    }
}