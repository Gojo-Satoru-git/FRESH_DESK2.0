using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Agent;

public class AgentConfiguration : IEntityTypeConfiguration<AgentEntity>
{
    public void Configure(EntityTypeBuilder<AgentEntity> builder)
    {
        builder.ToTable("agents", "agent");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.ProfilePhoto).HasColumnName("profile_photo");
        builder.Property(e => e.PreferredName).HasColumnName("preferred_name").HasMaxLength(100);
        builder.Property(e => e.EmployeeId).HasColumnName("employee_id").HasMaxLength(60);
        builder.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(150);
        builder.Property(e => e.ManagerAgentId).HasColumnName("manager_agent_id");
        builder.Property(e => e.MaxConcurrentTickets).HasColumnName("max_concurrent_tickets").HasDefaultValue(10);
        builder.Property(e => e.CurrentWorkload).HasColumnName("current_workload").HasDefaultValue(0);
        builder.Property(e => e.UtilizationPercentage).HasColumnName("utilization_percentage").HasColumnType("numeric(5,2)").HasDefaultValue(0);
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(40).HasDefaultValue("offline");
        builder.Property(e => e.StatusSince).HasColumnName("status_since");
        builder.Property(e => e.DeactivationReason).HasColumnName("deactivation_reason").HasMaxLength(500);
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne(d => d.Manager)
            .WithMany(p => p.DirectReports)
            .HasForeignKey(d => d.ManagerAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("idx_agents_user").HasFilter("is_deleted = false");
        builder.HasIndex(e => e.Status).HasDatabaseName("idx_agents_status").HasFilter("is_deleted = false");
        builder.HasIndex(e => e.ManagerAgentId).HasDatabaseName("idx_agents_manager").HasFilter("is_deleted = false");
    }
}