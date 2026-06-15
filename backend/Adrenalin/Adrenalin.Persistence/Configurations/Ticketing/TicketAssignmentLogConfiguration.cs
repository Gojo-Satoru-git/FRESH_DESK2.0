using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class TicketAssignmentLogConfiguration : IEntityTypeConfiguration<TicketAssignmentLog>
{
    public void Configure(EntityTypeBuilder<TicketAssignmentLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("ticket_assignment_log_pkey");

        builder.ToTable("ticket_assignment_log", "ticket");
        builder.HasQueryFilter(e => !e.Ticket.IsDeleted);

        builder.HasIndex(e => new { e.ToAgentId, e.AssignedAt }, "idx_tal_agent").IsDescending(false, true);

        builder.HasIndex(e => new { e.TicketId, e.AssignedAt }, "idx_tal_ticket").IsDescending(false, true);

        builder.HasIndex(e =>new { e.TicketId, e.FromAgentId, e.ToAgentId});

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.AssignedAt).HasColumnName("assigned_at");
        
        builder.Property(e => e.ChangedBy).HasColumnName("changed_by");
        
        builder.Property(e => e.FromAgentId).HasColumnName("from_agent_id");
        
        builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000); ;
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.ToAgentId).HasColumnName("to_agent_id");

        builder.Ignore(e => e.RowVersion);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.ChangedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_assignment_log_changed_by_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.FromAgentId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("ticket_assignment_log_from_agent_id_fkey");

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.ToAgentId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("ticket_assignment_log_to_agent_id_fkey");

        builder.HasOne(d => d.Ticket).WithMany(p => p.TicketAssignmentLogs).HasForeignKey(d => d.TicketId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("ticket_assignment_log_ticket_id_fkey");
    }
}
