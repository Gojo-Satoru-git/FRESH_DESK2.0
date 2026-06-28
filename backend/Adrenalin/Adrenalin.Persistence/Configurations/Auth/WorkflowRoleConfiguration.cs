// FILE: Adrenalin/Adrenalin.Persistence/Configurations/Auth/WorkflowRoleConfiguration.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Auth;

public class WorkflowRoleConfiguration : IEntityTypeConfiguration<WorkflowRole>
{
    public void Configure(EntityTypeBuilder<WorkflowRole> builder)
    {
        builder.HasKey(e => e.Id).HasName("workflow_roles_pkey");

        builder.ToTable("workflow_roles", "auth", tb => tb.HasComment(
            "FS-05 Workflow Role Master. Stage-eligibility catalogue only — carries no " +
            "permissions. Independent from auth.roles (Access Level). Do not join the two " +
            "in engine logic (BR-RP-004)."));

        // NFR-RP-006 — uniqueness enforced at DB level, case-insensitive.
        // Requires citext extension OR a computed lower(name) column — using the
        // expression-index approach here to avoid adding a new column type.
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("uq_workflow_roles_name_ci")
            .IsUnique()
            .HasFilter(null); // see migration SQL below for the actual lower(name) expression index

        builder.Property(e => e.Name).HasMaxLength(80).IsRequired().HasColumnName("name");
        builder.Property(e => e.Description).HasMaxLength(2000).HasColumnName("description");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(e => e.IsSystemDefault).HasColumnName("is_system_default").HasDefaultValue(false);
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.UserWorkflowRoles)
            .WithOne(uwr => uwr.WorkflowRole)
            .HasForeignKey(uwr => uwr.WorkflowRoleId)
            .OnDelete(DeleteBehavior.Restrict); // BR-RP-003 — never cascade-delete; handler must guard first
    }
}

/*
 * MIGRATION SQL — you said you apply schema changes manually, so here is the exact SQL
 * to run instead of `dotnet ef migrations add`. Run in this order.
 *
 * CREATE TABLE auth.workflow_roles (
 *     id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
 *     name             varchar(80) NOT NULL,
 *     description      varchar(2000) NULL,
 *     is_active        boolean NOT NULL DEFAULT true,
 *     is_system_default boolean NOT NULL DEFAULT false,
 *     is_deleted       boolean NOT NULL DEFAULT false,
 *     created_at       timestamptz NOT NULL DEFAULT now(),
 *     updated_at       timestamptz NULL,
 *     created_by       uuid NULL,
 *     updated_by       uuid NULL,
 *     row_version      bytea NULL
 * );
 *
 * -- Case-insensitive uniqueness, scoped to non-deleted rows only (NFR-RP-006 / FR-RP-002).
 * CREATE UNIQUE INDEX uq_workflow_roles_name_ci
 *     ON auth.workflow_roles (lower(name))
 *     WHERE is_deleted = false;
 */
