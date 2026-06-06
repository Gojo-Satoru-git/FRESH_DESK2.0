using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.SLA;

public class AutomationExecutionLogConfiguration : IEntityTypeConfiguration<AutomationExecutionLog>
{
    public void Configure(EntityTypeBuilder<AutomationExecutionLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("automation_execution_log_pkey");

        builder.ToTable("automation_execution_log", "sla");

        builder.HasIndex(e => new { e.Succeeded, e.ExecutedAt }, "idx_ael_failed").HasFilter("(succeeded = false)");

        builder.HasIndex(e => new { e.RuleId, e.ExecutedAt }, "idx_ael_rule").IsDescending(false, true);

        builder.HasIndex(e => new { e.TicketId, e.ExecutedAt }, "idx_ael_ticket").IsDescending(false, true);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.ErrorMessage).HasColumnName("error_message");
        
        builder.Property(e => e.ExecutedAt).HasDefaultValueSql("now()").HasColumnName("executed_at");
            
        builder.Property(e => e.RuleId).HasColumnName("rule_id");
        
        builder.Property(e => e.Succeeded).HasDefaultValue(true).HasColumnName("succeeded");
            
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");

        builder.HasOne(d => d.Rule).WithMany(p => p.AutomationExecutionLogs).HasForeignKey(d => d.RuleId).HasConstraintName("automation_execution_log_rule_id_fkey");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("automation_execution_log_ticket_id_fkey");
    }
}
