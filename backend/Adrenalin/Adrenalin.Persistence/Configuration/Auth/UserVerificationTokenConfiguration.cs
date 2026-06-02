using Adrenalin.Modules.Auth.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

public class UserVerificationTokenConfiguration
    : IEntityTypeConfiguration<UserVerificationToken>
{
    public void Configure(EntityTypeBuilder<UserVerificationToken> builder)
    {
        builder.ToTable(
            "user_verification_tokens",
            "auth",
            tb => tb.HasComment(
                "Hashed URL tokens for email verification and password reset flows."));

        builder.HasKey(x => x.Id)
               .HasName("user_verification_tokens_pkey");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_user_verification_tokens_user");

        builder.HasIndex(x => x.TokenHash)
               .HasDatabaseName("idx_user_verification_tokens_token_hash");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.TokenHash)
               .HasColumnName("token_hash")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Purpose)
               .HasColumnName("purpose")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.VerifiedAt)
               .HasColumnName("verified_at");

        builder.Property(x => x.IsUsed)
               .HasColumnName("is_used");

        builder.Property(x => x.CreatedByIp)
               .HasColumnName("created_by_ip")
               .HasMaxLength(100);

        // User Relationship

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserVerificationTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName(
                    "user_verification_tokens_user_id_fkey");
    }
}