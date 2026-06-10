using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

        builder.ToTable("refresh_tokens", "auth", tb => tb.HasComment("Stores hashed refresh tokens with family-based rotation tracking. On token reuse detection (possible theft), entire family_id is revoked immediately. token_hash is SHA-256 of raw token."));

        builder.HasIndex(e => e.ExpiresAt, "idx_refresh_tokens_expires").HasFilter("(is_revoked = false)");

        builder.HasIndex(e => e.FamilyId, "idx_refresh_tokens_family");

        builder.HasIndex(e => e.UserId, "idx_refresh_tokens_user").HasFilter("(is_revoked = false)");

        builder.HasIndex(e => e.TokenHash, "uq_refresh_tokens_hash").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");


        builder.Property(e => e.DeviceInfo)
            .HasMaxLength(255)
            .HasColumnName("device_info");

        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");

        builder.Property(e => e.FamilyId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("family_id");

        builder.Property(e => e.IpAddress).HasColumnName("ip_address");
        builder.Property(e => e.IsRevoked).HasColumnName("is_revoked");

        builder.Property(e => e.IssuedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("issued_at");

        builder.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
        builder.Property(e => e.ReplacedByTokenId).HasColumnName("replaced_by_token_id");
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");

        builder.Property(e => e.TokenHash)
            .HasMaxLength(255)
            .HasColumnName("token_hash");


        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.ReplacedByToken).WithMany(p => p.InverseReplacedByToken)
            .HasForeignKey(d => d.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("refresh_tokens_replaced_by_token_id_fkey");

        builder.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("refresh_tokens_user_id_fkey");
        builder.Property(e => e.UserAgent)
    .HasColumnName("user_agent");

builder.Property(e => e.RotatedAt)
    .HasColumnName("rotated_at");

builder.Property(e => e.CreatedByIp)
    .HasColumnName("created_by_ip");

builder.Property(e => e.RevokedByIp)
    .HasColumnName("revoked_by_ip");

builder.Property(e => e.RevokedReason)
    .HasColumnName("revoked_reason");
builder.Ignore(e => e.RowVersion);
builder.Property(e => e.UserAgent)
    .HasColumnName("user_agent");

builder.Property(e => e.CreatedByIp)
    .HasMaxLength(45)
    .HasColumnName("created_by_ip");

builder.Property(e => e.RevokedByIp)
    .HasMaxLength(45)
    .HasColumnName("revoked_by_ip");

builder.Property(e => e.RotatedAt)
    .HasColumnName("rotated_at");

builder.Property(e => e.RevokedReason)
    .HasColumnName("revoked_reason");
        
    }
}
