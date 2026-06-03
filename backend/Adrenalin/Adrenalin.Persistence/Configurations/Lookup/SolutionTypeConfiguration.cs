using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Lookup.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Lookup;

public class SolutionTypeConfiguration : IEntityTypeConfiguration<SolutionType>
{
    public void Configure(EntityTypeBuilder<SolutionType> builder)
    {
        builder.HasKey(e => e.Id).HasName("solution_types_pkey");

        builder.ToTable("solution_types", "lookup", tb => tb.HasComment("Valid solution_type values: data_correction, patch_deployment, configuration, clarification, server_outage, ad_hoc, known_issue. Replaces free-text varchar on tickets."));

        builder.HasIndex(e => e.Code, "uq_solution_types_code").IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.Code)
            .HasMaxLength(60)
            .HasColumnName("code");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(e => e.Label)
            .HasMaxLength(100)
            .HasColumnName("label");
    }
}
