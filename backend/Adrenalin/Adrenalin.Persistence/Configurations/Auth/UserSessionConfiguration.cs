using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_sessions_pkey");

        builder.ToTable("user_sessions", "auth", tb => tb.HasComment("One row per device/login. Enables security dashboard with all active devices. last_active_at updated by API middleware on each authenticated call."));

        builder.HasIndex(e => e.RefreshTokenId, "idx_user_sessions_token");

        builder.HasIndex(e => e.UserId, "idx_user_sessions_user").HasFilter("(is_active = true)");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.DeviceName)
            .HasMaxLength(150)
            .HasColumnName("device_name");

        builder.Property(e => e.EndedAt).HasColumnName("ended_at");

        builder.Property(e => e.GeoLocation)
            .HasMaxLength(100)
            .HasColumnName("geo_location");

        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.LastActiveAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("last_active_at");

        builder.Property(e => e.RefreshTokenId).HasColumnName("refresh_token_id");

        builder.Property(e => e.StartedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("started_at");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.RefreshToken).WithMany(p => p.UserSessions)
            .HasForeignKey(d => d.RefreshTokenId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_sessions_refresh_token_id_fkey");

        builder.HasOne(d => d.User).WithMany(p => p.UserSessions)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_sessions_user_id_fkey");
    }
}
