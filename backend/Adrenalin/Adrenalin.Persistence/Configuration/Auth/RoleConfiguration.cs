
using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(
            "roles",
            "auth",
            tb => tb.HasComment(
                "Named roles: junior_agent, team_lead, manager, admin, collaborator, pmo. System roles (is_system_role=true) cannot be deleted via admin UI."));

        builder.HasKey(x => x.Id)
               .HasName("roles_pkey");

        builder.HasIndex(x => x.Name)
               .IsUnique()
               .HasDatabaseName("uq_roles_name_exact");

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .HasMaxLength(80)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasColumnName("description");

        builder.Property(x => x.IsSystemRole)
               .HasColumnName("is_system_role");

        builder.Property(x => x.IsDeleted)
               .HasColumnName("is_deleted");

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
               .HasColumnName("updated_by");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at");

        builder.Property(x => x.RowVersion)
       .HasColumnName("row_version")
       .IsRowVersion()
       .IsConcurrencyToken();
       builder.HasQueryFilter(x => !x.IsDeleted);
        // Audit Relationships

        builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.CreatedBy)
       .OnDelete(DeleteBehavior.SetNull);

builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.UpdatedBy)
       .OnDelete(DeleteBehavior.SetNull);

        // Collections

        builder.HasMany(x => x.RolePermissions)
               .WithOne(x => x.Role)
               .HasForeignKey(x => x.RoleId);

        builder.HasMany(x => x.UserRoles)
               .WithOne(x => x.Role)
               .HasForeignKey(x => x.RoleId);
    }
}