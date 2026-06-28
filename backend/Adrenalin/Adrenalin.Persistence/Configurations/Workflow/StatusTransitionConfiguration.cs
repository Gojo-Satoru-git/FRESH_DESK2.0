using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Workflow.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Workflow;

public class StatusTransitionConfiguration : IEntityTypeConfiguration<StatusTransition>
{
    public void Configure(EntityTypeBuilder<StatusTransition> builder)
    {
        builder.HasKey(e => e.Id).HasName("status_transitions_pkey");

        builder.ToTable("status_transitions", "workflow", tb => tb.HasComment("One row per allowed edge in a graph. API layer validates every status change against this table. requires_field check: named tickets column must be non-null before transition allowed. auto_trigger=true allows automation engine to fire without human actor."));

        builder.HasIndex(e => new { e.GraphId, e.FromStatus }, "idx_status_transitions_from").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.GraphId, "idx_status_transitions_graph").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.GraphId, e.FromStatus, e.ToStatus }, "uq_status_transitions").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AutoTrigger).HasColumnName("auto_trigger");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.DisplayOrder).HasColumnName("display_order");
        
        builder.Property(e => e.FromStatus).HasMaxLength(40).HasColumnName("from_status");
        
        builder.Property(e => e.GraphId).HasColumnName("graph_id");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.RequiresField).HasMaxLength(100).HasColumnName("requires_field");

        builder.Property(e => e.RequiresRoleId).HasColumnName("requires_role_id");

        builder.Property(e => e.ToStatus).HasMaxLength(40).HasColumnName("to_status");
        
        builder.Ignore(e => e.RowVersion);
        
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("status_transitions_created_by_fkey");

        builder.HasOne(d => d.Graph).WithMany(p => p.StatusTransitions).HasForeignKey(d => d.GraphId).HasConstraintName("status_transitions_graph_id_fkey");

        builder.HasOne<Adrenalin.Modules.Auth.Domain.Entities.Role>()
        .WithMany()
        .HasForeignKey(e => e.RequiresRoleId)
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("status_transitions_requires_role_id_fkey");
    }
}
