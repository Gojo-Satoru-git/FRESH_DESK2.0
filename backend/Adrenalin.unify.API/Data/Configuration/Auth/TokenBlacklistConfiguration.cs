using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class TokenBlacklistConfiguration
    : IEntityTypeConfiguration<TokenBlacklist>
{
    public void Configure(EntityTypeBuilder<TokenBlacklist> builder)
    {
        builder.ToTable(
            "token_blacklists",
            "auth",
            tb => tb.HasComment(
                "Revoked JWT IDs. Auth middleware performs O(1) lookup on Jti before accepting any token."));

        builder.HasKey(x => x.Id)
               .HasName("token_blacklists_pkey");

        builder.HasIndex(x => x.Jti)
               .IsUnique()
               .HasDatabaseName("uq_token_blacklists_jti");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_token_blacklists_user");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Jti)
               .HasColumnName("jti")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at");

        builder.Property(x => x.Reason)
               .HasColumnName("reason");

        builder.Property(x => x.BlacklistedAt)
               .HasColumnName("blacklisted_at")
               .HasDefaultValueSql("now()");

        // User Relationship

        builder.HasOne(x => x.User)
               .WithMany(x => x.TokenBlacklists)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("token_blacklists_user_id_fkey");
    }
}