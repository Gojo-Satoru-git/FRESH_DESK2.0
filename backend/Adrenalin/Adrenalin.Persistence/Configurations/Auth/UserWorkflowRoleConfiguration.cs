// FILE: Adrenalin/Adrenalin.Persistence/Configurations/Auth/UserWorkflowRoleConfiguration.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.Auth;

public class UserWorkflowRoleConfiguration : IEntityTypeConfiguration<UserWorkflowRole>
{
    public void Configure(EntityTypeBuilder<UserWorkflowRole> builder)
    {
        builder.HasKey(e => e.Id).HasName("user_workflow_roles_pkey");

        builder.ToTable("user_workflow_roles", "auth", tb => tb.HasComment(
            "FS-05 §3.4 — join table for an agent's Primary Role (is_primary=true, exactly " +
            "one active row per user) and Additional Roles (is_primary=false, zero or more)."));

        builder.HasIndex(e => new { e.UserId, e.WorkflowRoleId })
            .HasDatabaseName("uq_user_workflow_roles_user_role")
            .IsUnique()
            .HasFilter("is_deleted = false");

        // FR-RP-041 equivalent for Workflow Role: at most one PRIMARY row per user.
        // Partial unique index — only one row per user where is_primary = true.
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("uq_user_workflow_roles_one_primary")
            .IsUnique()
            .HasFilter("is_primary = true AND is_deleted = false");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.WorkflowRoleId).HasColumnName("workflow_role_id");
        builder.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.WorkflowRole)
            .WithMany(wr => wr.UserWorkflowRoles)
            .HasForeignKey(e => e.WorkflowRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/*
 * MIGRATION SQL — manual apply.
 *
 * CREATE TABLE auth.user_workflow_roles (
 *     id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
 *     user_id          uuid NOT NULL REFERENCES auth.users(id),
 *     workflow_role_id uuid NOT NULL REFERENCES auth.workflow_roles(id),
 *     is_primary       boolean NOT NULL DEFAULT false,
 *     is_deleted       boolean NOT NULL DEFAULT false,
 *     created_at       timestamptz NOT NULL DEFAULT now(),
 *     updated_at       timestamptz NULL,
 *     created_by       uuid NULL,
 *     updated_by       uuid NULL,
 *     row_version      bytea NULL
 * );
 *
 * CREATE UNIQUE INDEX uq_user_workflow_roles_user_role
 *     ON auth.user_workflow_roles (user_id, workflow_role_id)
 *     WHERE is_deleted = false;
 *
 * -- Enforces "exactly one Primary Role" at the DB level, not just in application code.
 * CREATE UNIQUE INDEX uq_user_workflow_roles_one_primary
 *     ON auth.user_workflow_roles (user_id)
 *     WHERE is_primary = true AND is_deleted = false;
 */
