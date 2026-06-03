using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserVerificationTokenConfiguration : IEntityTypeConfiguration<UserVerificationToken>
{
    public void Configure(EntityTypeBuilder<UserVerificationToken> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_verification_tokens_pkey");

        builder.ToTable("user_verification_tokens", "auth", tb => tb.HasComment("Hashed URL tokens for email verification and password reset flows. is_used=true after first use (tokens are single-use). Expired rows purged by nightly cleanup job alongside refresh_tokens."));

        builder.HasIndex(e => e.ExpiresAt, "idx_vtoken_expires").HasFilter("(is_used = false)");

        builder.HasIndex(e => new { e.UserId, e.Purpose }, "idx_vtoken_user_purpose").HasFilter("(is_used = false)");

        builder.HasIndex(e => e.TokenHash, "uq_verification_token_hash").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(45)
            .HasColumnName("created_by_ip");

        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.IsUsed).HasColumnName("is_used");

        builder.Property(e => e.Purpose)
            .HasMaxLength(60)
            .HasColumnName("purpose");

        builder.Property(e => e.TokenHash)
            .HasMaxLength(255)
            .HasColumnName("token_hash");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");

        builder.HasOne(d => d.User).WithMany(p => p.UserVerificationTokens)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_verification_tokens_user_id_fkey");
    }
}
