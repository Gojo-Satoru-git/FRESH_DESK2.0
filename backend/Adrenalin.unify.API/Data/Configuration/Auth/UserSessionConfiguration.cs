using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth;

public class UserSessionConfiguration
    : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable(
            "user_sessions",
            "auth",
            tb => tb.HasComment(
                "One row per device/login. Enables security dashboard with all active devices."));

        builder.HasKey(x => x.Id)
               .HasName("user_sessions_pkey");

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("idx_user_sessions_user");

        builder.HasIndex(x => x.RefreshTokenId)
               .HasDatabaseName("idx_user_sessions_refresh_token");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
               .HasColumnName("user_id");

        builder.Property(x => x.RefreshTokenId)
               .HasColumnName("refresh_token_id");

        builder.Property(x => x.DeviceName)
               .HasColumnName("device_name")
               .HasMaxLength(255);

        builder.Property(x => x.IpAddress)
               .HasColumnName("ip_address");

        builder.Property(x => x.GeoLocation)
               .HasColumnName("geo_location")
               .HasMaxLength(255);

        builder.Property(x => x.StartedAt)
               .HasColumnName("started_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.LastActiveAt)
               .HasColumnName("last_active_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.EndedAt)
               .HasColumnName("ended_at");

        builder.Property(x => x.IsActive)
               .HasColumnName("is_active");

        // User Relationship

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserSessions)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("user_sessions_user_id_fkey");

        // Refresh Token Relationship

        builder.HasOne(x => x.RefreshToken)
               .WithMany(x => x.UserSessions)
               .HasForeignKey(x => x.RefreshTokenId)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("user_sessions_refresh_token_id_fkey");
    }
}