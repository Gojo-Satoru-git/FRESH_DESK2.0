using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class TokenBlacklistConfiguration : IEntityTypeConfiguration<TokenBlacklist>
{
    public void Configure(EntityTypeBuilder<TokenBlacklist> builder)
    {
        builder.HasKey(e => e.Id).HasName("token_blacklist_pkey");

        builder.ToTable("token_blacklist", "auth", tb => tb.HasComment("Revoked JWT IDs. Auth middleware performs O(1) lookup on jti before accepting any token. Rows pruned nightly: DELETE FROM auth.token_blacklist WHERE expires_at < NOW()."));

        builder.HasIndex(e => e.ExpiresAt, "idx_token_blacklist_expires");

        builder.HasIndex(e => e.UserId, "idx_token_blacklist_user");

        builder.HasIndex(e => e.Jti, "uq_token_blacklist_jti").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.BlacklistedAt)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamptz")
            .HasColumnName("blacklisted_at");

        builder.Property(e => e.ExpiresAt)
            .HasColumnType("timestamptz")
            .HasColumnName("expires_at");

        builder.Property(e => e.Jti)
            .HasMaxLength(36)
            .HasColumnName("jti");

        builder.Property(e => e.Reason)
            .HasMaxLength(100)
            .HasColumnName("reason");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.User).WithMany(p => p.TokenBlacklists)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("token_blacklist_user_id_fkey");
    }
}
