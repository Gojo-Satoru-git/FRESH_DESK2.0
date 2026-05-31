using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Adrenalin.unify.API.Models.AuthModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.unify.API.Data.Configuration.Auth
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups", "auth");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
       .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(x => x.RegionCode)
            .HasColumnName("region_code");

        builder.Property(x => x.TierCode)
            .HasColumnName("tier_code");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        // Audit Relationships

        builder.HasOne(x => x.CreatedByNavigation)
            .WithMany(x => x.GroupCreatedByNavigations)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.UpdatedByNavigation)
            .WithMany(x => x.GroupUpdatedByNavigations)
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Lookup Relationships

        builder.HasOne(x => x.RegionCodeNavigation)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.RegionCode);

        builder.HasOne(x => x.TierCodeNavigation)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.TierCode);

        // User Groups

        builder.HasMany(x => x.UserGroups)
            .WithOne(x => x.Group)
            .HasForeignKey(x => x.GroupId);
    }
}
}