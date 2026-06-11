using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Workflow.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Workflow;

public class TicketStatusGraphConfiguration : IEntityTypeConfiguration<TicketStatusGraph>
{
    public void Configure(EntityTypeBuilder<TicketStatusGraph> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_status_graphs_pkey");

        builder.ToTable("ticket_status_graphs", "workflow", tb => tb.HasComment("Named status transition graphs. graph_id is resolved once at ticket creation via ticket_status_graph_scopes priority engine and stored on tickets.graph_id. Subsequent transitions are validated against this graph_id only."));

        builder.HasIndex(e => e.IsActive, "idx_tsg_active").HasFilter("(is_deleted = false)");

        builder.HasIndex(e => e.GraphCode, "uq_ticket_status_graphs_code").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        
        builder.Property(e => e.Description).HasColumnName("description");
        
        builder.Property(e => e.GraphCode).HasMaxLength(40).HasColumnName("graph_code");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
        
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(e => e.Name).HasMaxLength(150).HasColumnName("name");
        
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        
        builder.Ignore(e => e.RowVersion);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graphs_created_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UpdatedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_status_graphs_updated_by_fkey");
    }
}
