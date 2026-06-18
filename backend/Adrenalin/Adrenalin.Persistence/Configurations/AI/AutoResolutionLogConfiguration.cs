using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.AI.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.AI;

public class AutoResolutionLogConfiguration : IEntityTypeConfiguration<AutoResolutionLog>
{
    public void Configure(EntityTypeBuilder<AutoResolutionLog> builder)
    {
        builder.HasKey(e => e.Id).HasName("auto_resolution_log_pkey");

        builder.ToTable("auto_resolution_log", "ai");

        builder.Ignore(e => e.RowVersion);

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        builder.Property(e => e.Suggestion).HasColumnName("suggestion");
        builder.Property(e => e.Applied).HasColumnName("applied");
        builder.Property(e => e.AppliedBy).HasColumnName("applied_by");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.HasIndex(e => e.TicketId, "idx_auto_res_ticket");

        builder.HasOne<Ticket>().WithMany().HasForeignKey(d => d.TicketId).HasConstraintName("auto_resolution_log_ticket_id_fkey");
        
        builder.HasOne<User>().WithMany().HasForeignKey(d => d.AppliedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("auto_resolution_log_applied_by_fkey");
    }
}
