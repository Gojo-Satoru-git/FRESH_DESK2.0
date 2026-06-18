using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class GroupAssignmentHistoryConfiguration : IEntityTypeConfiguration<GroupAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<GroupAssignmentHistory> builder)
    {
        builder.HasKey(e => e.Id).HasName("group_assignment_history_pkey");

        builder.ToTable("group_assignment_history", "ticket", tb =>
            tb.HasComment("Audit trail of every group assignment or reassignment for a ticket."));

        builder.HasIndex(e => e.TicketId, "idx_group_assignment_history_ticket");

        builder.HasIndex(e => e.NewGroupId, "idx_group_assignment_history_new_group")
            .HasFilter("(new_group_id IS NOT NULL)");

        builder.HasIndex(e => e.CreatedAt, "idx_group_assignment_history_created_at")
            .IsDescending();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        builder.Property(e => e.PreviousGroupId).HasColumnName("previous_group_id");
        builder.Property(e => e.NewGroupId).HasColumnName("new_group_id");
        builder.Property(e => e.AssignedBy).HasColumnName("assigned_by");

        builder.Property(e => e.Reason)
            .HasMaxLength(500)
            .HasColumnName("reason");

        builder.Property(e => e.RoutingRuleMatched)
            .HasMaxLength(200)
            .HasColumnName("routing_rule_matched");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.Ignore(e => e.RowVersion);

        // FK → ticket.tickets
        builder.HasOne<Ticket>().WithMany()
            .HasForeignKey(d => d.TicketId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("group_assignment_history_ticket_id_fkey");

        // FK → auth.groups (previous)
        builder.HasOne<Group>().WithMany()
            .HasForeignKey(d => d.PreviousGroupId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("group_assignment_history_previous_group_id_fkey");

        // FK → auth.groups (new)
        builder.HasOne<Group>().WithMany()
            .HasForeignKey(d => d.NewGroupId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("group_assignment_history_new_group_id_fkey");

        // FK → assigned_by user
        builder.HasOne<User>().WithMany()
            .HasForeignKey(d => d.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("group_assignment_history_assigned_by_fkey");
    }
}
