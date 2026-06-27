// Ported from your branch unchanged — matches sla.sla_stage_configs exactly
// as created by schema_sla_v11_postgres.sql (Part 5).

using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.SharedKernel.Enums.SLA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configurations.SLA;

public sealed class SlaStageConfigConfiguration : IEntityTypeConfiguration<SlaStageConfig>
{
    public void Configure(EntityTypeBuilder<SlaStageConfig> builder)
    {
        builder.ToTable("sla_stage_configs", "sla");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StageCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StageName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TimerBehaviour)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SlaTimerBehaviour.Run);
        builder.Property(x => x.OverridePolicyId).IsRequired(false);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.CreatedBy).IsRequired(false);
        builder.Property(x => x.UpdatedBy).IsRequired(false);

        builder.HasIndex(x => x.StageCode).IsUnique()
            .HasFilter("is_deleted = false");
        builder.HasIndex(x => x.IsActive)
            .HasFilter("is_active = true AND is_deleted = false");

        builder.HasOne<SlaPolicy>().WithMany()
            .HasForeignKey(x => x.OverridePolicyId)
            .IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}
