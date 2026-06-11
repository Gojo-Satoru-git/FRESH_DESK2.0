using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Workflow.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Workflow;

public class TicketStatusGraphScopeConfiguration : IEntityTypeConfiguration<TicketStatusGraphScope>
{
    public void Configure(EntityTypeBuilder<TicketStatusGraphScope> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_status_graph_scopes_pkey");

        builder.ToTable("ticket_status_graph_scopes", "workflow", tb => tb.HasComment("Scope-to-graph mapping. NULL dimensions act as wildcards. Resolver evaluates rows by priority DESC and returns the first match. GRAPH-001 (priority=0) is the default fallback. Uses FK references to product_versions, modules, sub_modules — replaces old varchar fields."));

        builder.HasIndex(e => e.GraphId, "idx_tsgs_graph").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => new { e.Priority, e.VersionId, e.ModuleId, e.SubModuleId }, "idx_tsgs_resolution").IsDescending(true, false, false, false).HasFilter("((is_active = true) AND (is_deleted = false))");

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Department).HasMaxLength(80).HasColumnName("department");
        
        builder.Property(e => e.GraphId).HasColumnName("graph_id");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.ModuleId).HasColumnName("module_id");
        
        builder.Property(e => e.Priority).HasColumnName("priority");
        
        builder.Property(e => e.SubModuleId).HasColumnName("sub_module_id");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        
        builder.Property(e => e.VersionId).HasColumnName("version_id");
        
        builder.Ignore(e => e.RowVersion);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graph_scopes_created_by_fkey");

        builder.HasOne(d => d.Graph).WithMany(p => p.TicketStatusGraphScopes).HasForeignKey(d => d.GraphId).HasConstraintName("ticket_status_graph_scopes_graph_id_fkey");

        builder.HasOne<Module>().WithMany().HasForeignKey(d => d.ModuleId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graph_scopes_module_id_fkey");

        builder.HasOne<SubModule>().WithMany().HasForeignKey(d => d.SubModuleId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graph_scopes_sub_module_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graph_scopes_updated_by_fkey");

        builder.HasOne<ProductVersion>().WithMany().HasForeignKey(d => d.VersionId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graph_scopes_version_id_fkey");
    }
}
