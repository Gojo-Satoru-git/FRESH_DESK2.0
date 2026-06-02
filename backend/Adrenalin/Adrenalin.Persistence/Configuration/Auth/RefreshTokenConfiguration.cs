using Adrenalin.Modules.Auth.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(
            "refresh_tokens",
            "auth",
            tb => tb.HasComment(
                "Stores hashed refresh tokens with family-based rotation tracking."));

        builder.HasKey(x => x.Id)
               .HasName("refresh_tokens_pkey");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_refresh_tokens_user");

        builder.HasIndex(x => x.FamilyId)
               .HasDatabaseName("idx_refresh_tokens_family");

        builder.HasIndex(x => x.TokenHash)
               .IsUnique()
               .HasDatabaseName("uq_refresh_tokens_token_hash");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.TokenHash)
               .HasColumnName("token_hash")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.FamilyId)
               .HasColumnName("family_id");

        builder.Property(x => x.DeviceInfo)
               .HasColumnName("device_info");

        builder.Property(x => x.IpAddress)
               .HasColumnName("ip_address");

        builder.Property(x => x.IssuedAt)
               .HasColumnName("issued_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at");

        builder.Property(x => x.LastUsedAt)
               .HasColumnName("last_used_at");

        builder.Property(x => x.RotatedAt)
               .HasColumnName("rotated_at");

        builder.Property(x => x.IsRevoked)
               .HasColumnName("is_revoked");

        builder.Property(x => x.RevokedAt)
               .HasColumnName("revoked_at");

        builder.Property(x => x.ReplacedByTokenId)
               .HasColumnName("replaced_by_token_id");

        builder.Property(x => x.CreatedByIp)
               .HasColumnName("created_by_ip");

        builder.Property(x => x.RevokedByIp)
               .HasColumnName("revoked_by_ip");

        builder.Property(x => x.UserAgent)
               .HasColumnName("user_agent");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        // User Relationship

        builder.HasOne(x => x.User)
               .WithMany(x => x.RefreshTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("refresh_tokens_user_id_fkey");

        // Self Reference (Token Rotation Chain)

        builder.HasOne(x => x.ReplacedByToken)
               .WithMany(x => x.InverseReplacedByToken)
               .HasForeignKey(x => x.ReplacedByTokenId)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("refresh_tokens_replaced_by_token_id_fkey");

        // User Sessions

        builder.HasMany(x => x.UserSessions)
               .WithOne(x => x.RefreshToken)
               .HasForeignKey(x => x.RefreshTokenId);
    }
}