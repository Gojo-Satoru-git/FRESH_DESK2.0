using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "auth");

            builder.HasKey(x => x.Id)
                   .HasName("users_pkey");

            builder.Property(x => x.Id)
                   .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.CreatedBy)
                   .HasColumnName("created_by");

            builder.Property(x => x.UpdatedBy)
                   .HasColumnName("updated_by");

            builder.Property(x => x.Email)
                   .HasColumnName("email");

            builder.Property(x => x.Email)
           .HasColumnName("email");

            builder.Property(x => x.NormalizedEmail)
                   .HasColumnName("normalized_email");

            builder.Property(x => x.Username)
                   .HasColumnName("username");

            builder.Property(x => x.NormalizedUsername)
                   .HasColumnName("normalized_username");

            builder.Property(x => x.PasswordHash)
                   .HasColumnName("password_hash");

            builder.Property(x => x.FirstName)
                   .HasColumnName("first_name");

            builder.Property(x => x.LastName)
                   .HasColumnName("last_name");

            builder.Property(x => x.Phone)
                   .HasColumnName("phone");

            builder.Property(x => x.AvatarUrl)
                   .HasColumnName("avatar_url");

            builder.Property(x => x.EmailVerified)
                   .HasColumnName("email_verified");

            builder.Property(x => x.EmailVerifiedAt)
                   .HasColumnName("email_verified_at");

            builder.Property(x => x.PasswordChangedAt)
                   .HasColumnName("password_changed_at");

            builder.Property(x => x.IsActive)
                   .HasColumnName("is_active");

            builder.Property(x => x.FailedLoginAttempts)
                   .HasColumnName("failed_login_attempts");

            builder.Property(x => x.LockoutEnd)
                   .HasColumnName("lockout_end");

            builder.Property(x => x.LastLoginAt)
                   .HasColumnName("last_login_at");

            builder.Property(x => x.RowVersion)
       .HasColumnName("row_version")
       .IsRowVersion()
       .IsConcurrencyToken();

            builder.Property(x => x.IsDeleted)
                   .HasColumnName("is_deleted");

            builder.Property(x => x.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("now()");

            builder.Property(x => x.UpdatedAt)
                   .HasColumnName("updated_at");

       builder.HasQueryFilter(x => !x.IsDeleted);
            // SELF REFERENCE #1

           builder.Property(x => x.CreatedBy)
       .HasColumnName("created_by");

builder.Property(x => x.UpdatedBy)
       .HasColumnName("updated_by");
        }
    }
